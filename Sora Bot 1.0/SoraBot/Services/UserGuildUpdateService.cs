using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sora_Bot_1.SoraBot.Services;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Sora_Bot_1.SoraBot.Services
{
    public class UserGuildUpdateService
    {
        ConcurrentDictionary<ulong, ulong> updateChannelPreferenceDict =
            new ConcurrentDictionary<ulong, ulong>();

        private JsonSerializer jSerializer = new JsonSerializer();

        public UserGuildUpdateService()
        {
            InitializeLoader();
            LoadDatabase();
        }

        public async Task UserLeft(SocketGuildUser user)
        {
            ulong channelID;
            if (updateChannelPreferenceDict.TryGetValue(user.Guild.Id, out channelID))
            {
                await (user.Guild.GetChannel(channelID) as IMessageChannel).SendMessageAsync($"`{user.Username}` has left us :frowning:");
            }
        }

        public async Task UserJoined(SocketGuildUser user)
        {
            ulong channelID;
            if (updateChannelPreferenceDict.TryGetValue(user.Guild.Id, out channelID))
            {
                await (user.Guild.GetChannel(channelID) as IMessageChannel).SendMessageAsync($"Welcome {user.Mention} to **{user.Guild.Name}**!");
            }
        }

        public async Task SetChannel(CommandContext Context)
        {
            try
            {
                if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
                {
                    updateChannelPreferenceDict.TryAdd(Context.Guild.Id, Context.Channel.Id);
                }
                else
                {
                    updateChannelPreferenceDict.TryUpdate(Context.Guild.Id, Context.Channel.Id);
                }
                SaveDatabase();
                await Context.Channel.SendMessageAsync(
                    $"Successfully set Announcement Channel to `{Context.Channel.Name}`!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task RemoveChannel(CommandContext Context)
        {
            
            if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync("No Announcement Channel set in this Guild!");
            }
            else
            {
                ulong ignore;
                updateChannelPreferenceDict.TryRemove(Context.Guild.Id, out ignore);
                await Context.Channel.SendMessageAsync("Announcement Channel for this Guild was removed. No Announcements will be done anymore!");
                SaveDatabase();
            }
        }

        private void SaveDatabase()
        {
            try
            {
                using (StreamWriter sw = File.CreateText(@"AnnouncementChannels.json"))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        jSerializer.Serialize(writer, updateChannelPreferenceDict);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                SentryService.SendError(e);
            }
            
        }

        private void LoadDatabase()
        {
            try
            {
                if (File.Exists("AnnouncementChannels.json"))
                {
                    using (StreamReader sr = File.OpenText(@"AnnouncementChannels.json"))
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            updateChannelPreferenceDict =
                                jSerializer.Deserialize<ConcurrentDictionary<ulong, ulong>>(reader);
                        }
                    }
                }
                else
                {
                    File.Create("AnnouncementChannels.json").Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                SentryService.SendError(e);
            }
        }

        private void InitializeLoader()
        {
            jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }
    }
}
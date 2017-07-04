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
        ConcurrentDictionary<ulong, annoucementStruct> updateChannelPreferenceDict =
            new ConcurrentDictionary<ulong, annoucementStruct>();

        private readonly JsonSerializer jSerializer = new JsonSerializer();

        public UserGuildUpdateService()
        {
            InitializeLoader();
            LoadDatabase();
        }

        public async Task UserLeft(SocketGuildUser user)
        {
            try
            {
                annoucementStruct str = new annoucementStruct();
                if (updateChannelPreferenceDict.TryGetValue(user.Guild.Id, out str))
                {
                    IMessageChannel channel = user.Guild.GetChannel(str.leaveID) as IMessageChannel;
                    if (channel == null)
                    {
                        return;
                    }
                    if (String.IsNullOrWhiteSpace(str.leaveMsg))
                    {
                        await channel.SendMessageAsync($"`{user.Username}` has left us :frowning:");
                    }
                    else
                    {
                        await channel.SendMessageAsync(ReplaceInfo(user, str.leaveMsg));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        public async Task UserJoined(SocketGuildUser user)
        {
            try
            {
                annoucementStruct str = new annoucementStruct();
                if (updateChannelPreferenceDict.TryGetValue(user.Guild.Id, out str))
                {
                    IMessageChannel channel = user.Guild.GetChannel(str.channelID) as IMessageChannel;
                    if (channel == null)
                    {
                        return;
                    }
                    if (String.IsNullOrWhiteSpace(str.message))
                    {
                        await channel.SendMessageAsync($"Welcome {user.Mention} to **{user.Guild.Name}**!");
                    }
                    else
                    {
                        await channel.SendMessageAsync(ReplaceInfo(user, str.message));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
            
        }

        //WELCOME

        public async Task SetWelcome(SocketCommandContext Context, string message)
        {
            try
            {
                annoucementStruct str = new annoucementStruct();
                if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
                {
                    str.channelID=Context.Channel.Id;
                    str.message = message;
                    updateChannelPreferenceDict.TryAdd(Context.Guild.Id, str);
                }
                else
                {
                    updateChannelPreferenceDict.TryGetValue(Context.Guild.Id, out str);
                    str.channelID = Context.Channel.Id;
                    str.message = message;
                    updateChannelPreferenceDict.TryUpdate(Context.Guild.Id, str);
                }
                SaveDatabase();
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: Successfully set Welcome Channel to `{Context.Channel.Name}` with {(String.IsNullOrWhiteSpace(str.message) ? "the default leave message" : $"message:\n{str.message}")}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task SetWelcomeMessage (SocketCommandContext Context, string message)
        {
            try
            {
                annoucementStruct str = new annoucementStruct();
                if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
                {
                    str.message = message;
                    updateChannelPreferenceDict.TryAdd(Context.Guild.Id, str);
                }
                else
                {
                    updateChannelPreferenceDict.TryGetValue(Context.Guild.Id, out str);
                    str.message = message;
                    updateChannelPreferenceDict.TryUpdate(Context.Guild.Id, str);
                }
                SaveDatabase();
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: Successfully set Welcome Message to `{message}` and {(str.channelID!=0 ? ($"with Channel {((Context.Guild as SocketGuild).GetChannel(str.channelID)).Name}") : "with no channel yet!")}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task SetWelcomeChannel(SocketCommandContext Context, IMessageChannel channel)
        {
            try
            {
                annoucementStruct str = new annoucementStruct();
                if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
                {
                    str.channelID = channel.Id;
                    updateChannelPreferenceDict.TryAdd(Context.Guild.Id, str);
                }
                else
                {
                    updateChannelPreferenceDict.TryGetValue(Context.Guild.Id, out str);
                    str.channelID = channel.Id;
                    updateChannelPreferenceDict.TryUpdate(Context.Guild.Id, str);
                }
                SaveDatabase();
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: Successfully set Welcome Channel to `{channel.Name}`!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task RemoveWelcome(SocketCommandContext Context)
        {
            try
            {
                if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: No Announcement Channel set in this Guild!");
                }
                else
                {
                    annoucementStruct str = new annoucementStruct();
                    updateChannelPreferenceDict.TryGetValue(Context.Guild.Id, out str);
                    str.channelID = 0;
                    str.message = null;
                    updateChannelPreferenceDict.TryUpdate(Context.Guild.Id, str);
                    await Context.Channel.SendMessageAsync(":white_check_mark: Welcome Channel for this Guild was removed. No Announcements will be done anymore!");
                    SaveDatabase();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }

        }

        //LEAVE 

        public async Task SetLeave(SocketCommandContext Context, string message)
        {
            try
            {
                annoucementStruct str = new annoucementStruct();
                if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
                {
                    str.leaveID = Context.Channel.Id;
                    str.leaveMsg = message;
                    updateChannelPreferenceDict.TryAdd(Context.Guild.Id, str);
                }
                else
                {
                    updateChannelPreferenceDict.TryGetValue(Context.Guild.Id, out str);
                    str.leaveID = Context.Channel.Id;
                    str.leaveMsg = message;
                    updateChannelPreferenceDict.TryUpdate(Context.Guild.Id, str);
                }
                SaveDatabase();
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: Successfully set Leave Channel to `{Context.Channel.Name}` with {(String.IsNullOrWhiteSpace(str.leaveMsg) ? "the default leave message" : $"message:\n{str.leaveMsg}" )}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task SetLeaveMessage(SocketCommandContext Context, string message)
        {
            try
            {
                annoucementStruct str = new annoucementStruct();
                if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
                {
                    str.leaveMsg = message;
                    updateChannelPreferenceDict.TryAdd(Context.Guild.Id, str);
                }
                else
                {
                    updateChannelPreferenceDict.TryGetValue(Context.Guild.Id, out str);
                    str.leaveMsg = message;
                    updateChannelPreferenceDict.TryUpdate(Context.Guild.Id, str);
                }
                SaveDatabase();
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: Successfully set Leave Message to `{message}` and {(str.leaveID != 0 ? ($"with Channel {((Context.Guild as SocketGuild).GetChannel(str.leaveID)).Name}") : "with no channel yet!")}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task SetLeaveChannel(SocketCommandContext Context, IMessageChannel channel)
        {
            try
            {
                annoucementStruct str = new annoucementStruct();
                if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
                {
                    str.leaveID = channel.Id;
                    updateChannelPreferenceDict.TryAdd(Context.Guild.Id, str);
                }
                else
                {
                    updateChannelPreferenceDict.TryGetValue(Context.Guild.Id, out str);
                    str.leaveID = channel.Id;
                    updateChannelPreferenceDict.TryUpdate(Context.Guild.Id, str);
                }
                SaveDatabase();
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: Successfully set Leave Channel to `{channel.Name}`!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task RemoveLeave(SocketCommandContext Context)
        {
            try
            {
                if (!updateChannelPreferenceDict.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: No Announcement Channel set in this Guild!");
                }
                else
                {
                    annoucementStruct str = new annoucementStruct();
                    updateChannelPreferenceDict.TryGetValue(Context.Guild.Id, out str);
                    str.leaveID = 0;
                    str.leaveMsg = null;
                    updateChannelPreferenceDict.TryUpdate(Context.Guild.Id, str);
                    await Context.Channel.SendMessageAsync(":white_check_mark: Leave Channel for this Guild was removed. No Announcements will be done anymore!");
                    SaveDatabase();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }

        }


        private string ReplaceInfo(SocketGuildUser user, string message)
        {
            var edited = message.Replace("{user}", $"{user.Mention}");
            edited = edited.Replace("{user#}", $"{user.Username}#{user.Discriminator}");
            edited = edited.Replace("{server}", $"{user.Guild.Name}");
            edited = edited.Replace("{count}", $"{user.Guild.MemberCount}");
            return edited;
        }

        

        public struct annoucementStruct
        {
            public ulong channelID;
            public ulong leaveID;
            public string message;
            public string leaveMsg;
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
                            var temp = jSerializer.Deserialize<ConcurrentDictionary<ulong, annoucementStruct>>(reader);
                            if (temp == null)
                                return;
                            updateChannelPreferenceDict = temp;
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
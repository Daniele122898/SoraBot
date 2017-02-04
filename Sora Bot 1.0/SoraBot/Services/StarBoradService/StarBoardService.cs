using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sora_Bot_1.SoraBot.Services.StarBoradService
{
    public class StarBoardService
    {
        private ConcurrentDictionary<ulong, ulong> starChannelDict = new ConcurrentDictionary<ulong, ulong>();

        private readonly JsonSerializer jSerializer = new JsonSerializer();

        public StarBoardService()
        {
            InitializeLoader();
            LoadDatabase();
        }

        public async Task StarAdded(ulong msgID, Optional<SocketUserMessage> msg, SocketReaction reaction)
        {
            try
            {
                ulong channelID;
                IMessage dmsg = null;
                bool specified = true;
                ulong guildID = (reaction.Channel as IGuildChannel).GuildId;

                if (starChannelDict.TryGetValue(guildID, out channelID))
                {
                    if (reaction.Emoji.Name.Equals("⭐") || reaction.Emoji.Name.Equals("🌟"))
                    {
                        if (!msg.IsSpecified) //!String.IsNullOrEmpty(tag)
                        {
                            dmsg = await reaction.Channel.GetMessageAsync(msgID, CacheMode.AllowDownload, null);
                            specified = false;
                        }

                        if (reaction.UserId == (specified ? reaction.Message.Value.Author.Id : dmsg.Author.Id))
                        {
                            return;
                        }

                        if ((specified ? reaction.Message.Value.Attachments.Count : dmsg.Attachments.Count) < 1)
                        {
                            var eb = new EmbedBuilder()
                            {
                                Color = new Color(4, 97, 247),
                                Timestamp = DateTimeOffset.UtcNow,
                                Author = new EmbedAuthorBuilder()
                                {
                                    IconUrl =
                                        (specified ? reaction.Message.Value.Author.AvatarUrl : dmsg.Author.AvatarUrl),
                                    Name =
                                        $"{(specified ? reaction.Message.Value.Author.Username : dmsg.Author.Username)}#{(specified ? reaction.Message.Value.Author.Discriminator : dmsg.Author.Discriminator)}"
                                },
                                Description =
                                    (specified ? reaction.Message.Value.Content : dmsg.Content)
                            };
                            await (((reaction.Channel as IGuildChannel).Guild as SocketGuild).GetChannel(channelID) as
                                IMessageChannel).SendMessageAsync(
                                $"{reaction.Emoji.Name} in #{((specified ? reaction.Message.GetValueOrDefault().Channel : dmsg.Channel) as IMessageChannel).Name}\n",
                                false, eb);
                        }
                        else
                        {
                            var eb = new EmbedBuilder()
                            {
                                Color = new Color(4, 97, 247),
                                Timestamp = DateTimeOffset.UtcNow,
                                ImageUrl =
                                (specified
                                    ? reaction.Message.Value.Attachments.FirstOrDefault().Url
                                    : dmsg.Attachments.FirstOrDefault().Url),
                                Author = new EmbedAuthorBuilder()
                                {
                                    IconUrl =
                                        (specified ? reaction.Message.Value.Author.AvatarUrl : dmsg.Author.AvatarUrl),
                                    Name =
                                        $"{(specified ? reaction.Message.Value.Author.Username : dmsg.Author.Username)}#{(specified ? reaction.Message.Value.Author.Discriminator : dmsg.Author.Discriminator)}"
                                },
                                Description =
                                    (specified ? reaction.Message.Value.Content : dmsg.Content)
                            };
                            await (((reaction.Channel as IGuildChannel).Guild as SocketGuild).GetChannel(channelID) as
                                IMessageChannel).SendMessageAsync(
                                $"{reaction.Emoji.Name} in #{((specified ? reaction.Message.GetValueOrDefault().Channel : dmsg.Channel) as IMessageChannel).Name}\n",
                                false, eb);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        public async Task StarRemoved(ulong arg1, Optional<SocketUserMessage> arg2, SocketReaction arg3)
        {
        }

        public async Task SetChannel(CommandContext Context)
        {
            try
            {
                if (!starChannelDict.ContainsKey(Context.Guild.Id))
                {
                    starChannelDict.TryAdd(Context.Guild.Id, Context.Channel.Id);
                }
                else
                {
                    starChannelDict.TryUpdate(Context.Guild.Id, Context.Channel.Id);
                }
                SaveDatabase();
                await Context.Channel.SendMessageAsync(
                    $"Successfully set StarBoard Channel to `{Context.Channel.Name}`!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task RemoveChannel(CommandContext Context)
        {
            if (!starChannelDict.ContainsKey(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync("No StarBoard Channel set in this Guild!");
            }
            else
            {
                ulong ignore;
                starChannelDict.TryRemove(Context.Guild.Id, out ignore);
                await Context.Channel.SendMessageAsync(
                    "StarBoard Channel for this Guild was removed. No StarReactions will be posted anymore!");
                SaveDatabase();
            }
        }

        private void SaveDatabase()
        {
            try
            {
                using (StreamWriter sw = File.CreateText(@"StarBoard.json"))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        jSerializer.Serialize(writer, starChannelDict);
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
                if (File.Exists("StarBoard.json"))
                {
                    using (StreamReader sr = File.OpenText(@"StarBoard.json"))
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            starChannelDict =
                                jSerializer.Deserialize<ConcurrentDictionary<ulong, ulong>>(reader);
                        }
                    }
                }
                else
                {
                    File.Create("StarBoard.json").Dispose();
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
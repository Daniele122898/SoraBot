using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sora_Bot_1.SoraBot.Services.StarBoradService
{
    public class StarBoardService
    {
        private ConcurrentDictionary<ulong, ulong> starChannelDict = new ConcurrentDictionary<ulong, ulong>();
        private ConcurrentDictionary<ulong, starMsg> msgIdDictionary = new ConcurrentDictionary<ulong, starMsg>();
        private ConcurrentDictionary<ulong, short> msgStarBlackDict = new ConcurrentDictionary<ulong, short>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, short>> userReactionBlacklistDict =
            new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, short>>();

        private readonly JsonSerializer jSerializer = new JsonSerializer();

        private DiscordSocketClient client;

        public StarBoardService(DiscordSocketClient c)
        {
            client = c;
            InitializeLoader();
            LoadDatabase();
        }

        public async Task StarAddedNew(Cacheable<IUserMessage, ulong> msgCacheable, ISocketMessageChannel msgChannel, SocketReaction reaction)
        {
            try
            {
                var msg = await msgCacheable.GetOrDownloadAsync();
                //MSG BLACKLIST
                if (msgStarBlackDict.ContainsKey(msg.Id))
                {
                    short count;
                    msgStarBlackDict.TryGetValue(msg.Id, out count);
                    if (count >= 2)
                    {
                        return;
                    }
                }

                ulong channelID;
                IUserMessage sentMessage = null;
                //IMessage dmsg = null;
                //bool specified = true;
                ulong guildID = (reaction.Channel as IGuildChannel).GuildId;

                if (starChannelDict.TryGetValue(guildID, out channelID))
                {
                    if (reaction.Emoji.Name.Equals("⭐") || reaction.Emoji.Name.Equals("🌟"))
                    {
                        /*
                        if (!msg.IsSpecified) //!String.IsNullOrEmpty(tag)
                        {
                            dmsg = await reaction.Channel.GetMessageAsync(msgID, CacheMode.AllowDownload, null);
                            specified = false;
                        }*/

                        if (reaction.UserId == msg.Author.Id)
                        {
                            return;
                        }

                        if (client.CurrentUser.Id == msg.Author.Id)
                        {
                            return;
                        }

                        Random rand = new Random(DateTime.Now.Millisecond);
                        await Task.Delay(rand.Next(50));

                        if (msgIdDictionary.ContainsKey(msg.Id))
                        {
                            //USER BLACKLIST
                            if (await CheckBlacklist(msg.Id, reaction.UserId))
                            {
                                return;
                            }

                            starMsg msgStruct = new starMsg();
                            msgIdDictionary.TryGetValue(msg.Id, out msgStruct);
                            msgStruct.counter += 1;
                            /*var guild = ((reaction.Channel as IGuildChannel)?.Guild as SocketGuild);
                            var channel = guild?.GetChannel(channelID) as IMessageChannel;
                            var msgToEdit =
                                (IUserMessage)
                                await channel.GetMessageAsync(msgStruct.starMSGID, CacheMode.AllowDownload, null);*/
                            msgIdDictionary.TryUpdate(msg.Id, msgStruct);
                            /*await msgToEdit.ModifyAsync(x => { x.Content = $"{msgStruct.counter} {msgToEdit.Content}"; });*/


                            SaveDatabase();
                            return;
                        }
                        else
                        {
                            //USER BLACKLIST
                            if (await CheckBlacklist(msg.Id, reaction.UserId))
                            {
                                return;
                            }


                            if (msg.Attachments.Count < 1)
                            {
                                var eb = new EmbedBuilder()
                                {
                                    Color = new Color(4, 97, 247),
                                    Timestamp = DateTimeOffset.UtcNow,
                                    Author = new EmbedAuthorBuilder()
                                    {
                                        IconUrl =
                                            msg.Author.GetAvatarUrl(),
                                        Name =
                                            $"{msg.Author.Username}#{msg.Author.Discriminator}"
                                    },
                                    Description = (msg.Content.Contains("http") ? "Link detected. Copied outside to embed link/picture" : msg.Content)
                                };
                                sentMessage = await (((reaction.Channel as IGuildChannel).Guild as SocketGuild)
                                    .GetChannel(channelID) as
                                    IMessageChannel).SendMessageAsync(
                                    $"{reaction.Emoji.Name} in #{msg.Channel.Name}{(msg.Content.Contains("http") ? $" by {msg.Author.Username}#{msg.Author.Discriminator}\n{msg.Content}" : "")}",
                                    false, (msg.Content.Contains("http") ? null : eb));

                            }
                            else
                            {
                                var eb = new EmbedBuilder()
                                {
                                    Color = new Color(4, 97, 247),
                                    Timestamp = DateTimeOffset.UtcNow,
                                    ImageUrl =
                                    msg.Attachments.FirstOrDefault().Url,
                                    Author = new EmbedAuthorBuilder()
                                    {
                                        IconUrl =
                                            msg.Author.GetAvatarUrl(),
                                        Name =
                                            $"{msg.Author.Username}#{msg.Author.Discriminator}"
                                    },
                                    Description =
                                        msg.Content
                                };
                                sentMessage = await (((reaction.Channel as IGuildChannel).Guild as SocketGuild)
                                    .GetChannel(channelID) as
                                    IMessageChannel).SendMessageAsync(
                                    $"{reaction.Emoji.Name} in #{msg.Channel.Name}\n",
                                    false, eb);
                            }
                            starMsg msgStruct = new starMsg
                            {
                                starMSGID = sentMessage.Id,
                                counter = 1
                            };
                            msgIdDictionary.TryAdd(msg.Id, msgStruct);
                            SaveDatabase();
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


        public async Task StarRemovedNew(Cacheable<IUserMessage, ulong> msgCacheable, ISocketMessageChannel msgChannel, SocketReaction reaction)
        {
            try
            {
                var msg = await msgCacheable.GetOrDownloadAsync();
                ulong channelID;
                ulong guildID = (reaction.Channel as IGuildChannel).GuildId;

                if (starChannelDict.TryGetValue(guildID, out channelID))
                {
                    starMsg msgStruct = new starMsg();
                    if (msgIdDictionary.TryGetValue(msg.Id, out msgStruct))
                    {
                        msgStruct.counter -= 1;
                        var guild = ((reaction.Channel as IGuildChannel)?.Guild as SocketGuild);
                        var channel = guild?.GetChannel(channelID) as IMessageChannel;
                        var msgToEdit =
                            (IUserMessage)
                            await channel.GetMessageAsync(msgStruct.starMSGID, CacheMode.AllowDownload, null);

                        if (msgStruct.counter < 1)
                        {
                            await msgToEdit.DeleteAsync();
                            msgIdDictionary.TryRemove(msg.Id, out msgStruct);
                            if (msgStarBlackDict.ContainsKey(msg.Id))
                            {
                                short count;
                                msgStarBlackDict.TryGetValue(msg.Id, out count);
                                count++;
                                msgStarBlackDict.TryUpdate(msg.Id, count);
                            }
                            else
                            {
                                msgStarBlackDict.TryAdd(msg.Id, 1);
                            }
                        }
                        else
                        {
                            /*
                            if (msgToEdit.Content.Contains("⭐"))
                            {
                                string subString = msgToEdit.Content.Substring(msgToEdit.Content.IndexOf("⭐"));
                                await msgToEdit.ModifyAsync(x => { x.Content = $"{msgStruct.counter} {subString}"; });
                            }
                            else if(msgToEdit.Content.Contains("🌟"))
                            {
                                string subString = msgToEdit.Content.Substring(msgToEdit.Content.IndexOf("🌟"));
                                await msgToEdit.ModifyAsync(x => { x.Content = $"{msgStruct.counter} {subString}"; });
                            }*/
                            msgIdDictionary.TryUpdate(msg.Id, msgStruct);
                        }
                        SaveDatabase();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
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

        public async Task<bool> CheckBlacklist(ulong msgID, ulong userID)
        {
            ConcurrentDictionary<ulong, short> userList = new ConcurrentDictionary<ulong, short>();
            if (userReactionBlacklistDict.ContainsKey(msgID))
            {
                userReactionBlacklistDict.TryGetValue(msgID, out userList);
                short counter;
                if (userList.ContainsKey(userID))
                {
                    userList.TryGetValue(userID, out counter);
                    if (counter >= 3)
                    {
                        return true;
                    }
                    else
                    {
                        counter++;
                        userList.TryUpdate(userID, counter);
                        userReactionBlacklistDict.TryUpdate(msgID, userList);
                    }
                }
                else
                {
                    counter = 1;
                    userList.TryAdd(userID, counter);
                    userReactionBlacklistDict.TryUpdate(msgID, userList);
                }
            }
            else
            {
                short counter = 1;
                userList.TryAdd(userID, counter);
                userReactionBlacklistDict.TryAdd(msgID, userList);
            }
            return false;
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

        public struct starMsg
        {
            public ulong starMSGID;
            public int counter;
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
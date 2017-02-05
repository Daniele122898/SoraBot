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

        public StarBoardService()
        {
            InitializeLoader();
            LoadDatabase();
        }

        public async Task StarAdded(ulong msgID, Optional<SocketUserMessage> msg, SocketReaction reaction)
        {
            try
            {
                //MSG BLACKLIST
                if (msgStarBlackDict.ContainsKey(msgID))
                {
                    short count;
                    msgStarBlackDict.TryGetValue(msgID, out count);
                    if (count >= 2)
                    {
                        return;
                    }
                }

                ulong channelID;
                IUserMessage sentMessage = null;
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

                        Random rand = new Random(DateTime.Now.Millisecond);
                        await Task.Delay(rand.Next(50));

                        if (msgIdDictionary.ContainsKey(msgID))
                        {
                            //USER BLACKLIST
                            if (await CheckBlacklist(msgID, reaction.UserId))
                            {
                                return;
                            }

                            starMsg msgStruct = new starMsg();
                            msgIdDictionary.TryGetValue(msgID, out msgStruct);
                            msgStruct.counter += 1;
                            /*var guild = ((reaction.Channel as IGuildChannel)?.Guild as SocketGuild);
                            var channel = guild?.GetChannel(channelID) as IMessageChannel;
                            var msgToEdit =
                                (IUserMessage)
                                await channel.GetMessageAsync(msgStruct.starMSGID, CacheMode.AllowDownload, null);*/
                            msgIdDictionary.TryUpdate(msgID, msgStruct);
                            /*await msgToEdit.ModifyAsync(x => { x.Content = $"{msgStruct.counter} {msgToEdit.Content}"; });*/


                            SaveDatabase();
                            return;
                        }
                        else
                        {
                            //USER BLACKLIST
                            if (await CheckBlacklist(msgID, reaction.UserId))
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
                                sentMessage = await (((reaction.Channel as IGuildChannel).Guild as SocketGuild)
                                    .GetChannel(channelID) as
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
                                sentMessage = await (((reaction.Channel as IGuildChannel).Guild as SocketGuild)
                                    .GetChannel(channelID) as
                                    IMessageChannel).SendMessageAsync(
                                    $"{reaction.Emoji.Name} in #{((specified ? reaction.Message.GetValueOrDefault().Channel : dmsg.Channel) as IMessageChannel).Name}\n",
                                    false, eb);
                            }
                            starMsg msgStruct = new starMsg
                            {
                                starMSGID = sentMessage.Id,
                                counter = 1
                            };
                            msgIdDictionary.TryAdd(msgID, msgStruct);
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

        public async Task StarRemoved(ulong msgID, Optional<SocketUserMessage> msg, SocketReaction reaction)
        {
            ulong channelID;
            ulong guildID = (reaction.Channel as IGuildChannel).GuildId;

            if (starChannelDict.TryGetValue(guildID, out channelID))
            {
                starMsg msgStruct = new starMsg();
                if (msgIdDictionary.TryGetValue(msgID, out msgStruct))
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
                        msgIdDictionary.TryRemove(msgID, out msgStruct);
                        if (msgStarBlackDict.ContainsKey(msgID))
                        {
                            short count;
                            msgStarBlackDict.TryGetValue(msgID, out count);
                            count++;
                            msgStarBlackDict.TryUpdate(msgID, count);
                        }
                        else
                        {
                            msgStarBlackDict.TryAdd(msgID, 1);
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
                        msgIdDictionary.TryUpdate(msgID, msgStruct);
                    }
                    SaveDatabase();
                }
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
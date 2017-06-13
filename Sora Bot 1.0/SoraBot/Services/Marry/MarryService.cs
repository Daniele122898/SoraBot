using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.YouTube.v3;

namespace Sora_Bot_1.SoraBot.Services.Marry
{
    public class MarryService
    {
        //                          User, list<waifus>
        private ConcurrentDictionary<ulong, List<MarryData>> _marryDict = new ConcurrentDictionary<ulong, List<MarryData>>();

        private MarryServiceDB _marryDB;

        private const int MAX_MARRIAGES = 10;

        public MarryService()
        {
            _marryDB = MarryServiceDB.Instance;
            _marryDB.InitializeLoader();
            _marryDict = _marryDB.LoadMarryData();
        }
        

        public async Task Marry(SocketCommandContext Context, InteractiveService interactive, SocketGuildUser user)
        {
            if (user.Id == Context.User.Id)
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You can't marry yourself ;_;");
                return;
            }

            List<MarryData> marryData = new List<MarryData>();
            if (_marryDict.ContainsKey(Context.User.Id))
            {
                _marryDict.TryGetValue(Context.User.Id, out marryData);
                if (marryData != null && marryData.Count > 0)
                {
                    if (marryData.FirstOrDefault(x => x.UserId == user.Id) != null)
                    {
                        await Context.Channel.SendMessageAsync(":no_entry_sign: You can't marry someone twice!");
                        return;
                    }

                    if (marryData.Count >= 10)
                    {
                        await Context.Channel.SendMessageAsync(":no_entry_sign: You can't marry more than 10 times!");
                        return; 
                    }
                }
            }

            await Context.Channel.SendMessageAsync($"{user.Mention}, do you want to marry {Context.User.Mention}? :ring:");
            var response = await interactive.WaitForMessage(user, Context.Channel, TimeSpan.FromSeconds(20));
            if (response == null || !response.Content.Equals("yes", StringComparison.CurrentCultureIgnoreCase))
            {
                await Context.Channel.SendMessageAsync($"{user.Username} has not answered with a Yes :frowning:");
                return;
            }

            await Context.Channel.SendMessageAsync($"You are now married :couple_with_heart:\n https://media.giphy.com/media/iQ5rGja9wWB9K/giphy.gif");

            var newMarriage = new MarryData
            {
                UserId = user.Id,
                MarriedSince = DateTime.UtcNow
            };

            marryData.Add(newMarriage);
            _marryDict.AddOrUpdate(Context.User.Id, marryData, (key, oldValue) => marryData);
            marryData = new List<MarryData>();
            if (_marryDict.ContainsKey(user.Id))
                _marryDict.TryGetValue(user.Id, out marryData);

            newMarriage = new MarryData
            {
                UserId = Context.User.Id,
                MarriedSince = DateTime.UtcNow
            };

            marryData.Add(newMarriage);
            _marryDict.AddOrUpdate(user.Id, marryData, (key, oldValue) => marryData);
            _marryDB.SaveMarryData(_marryDict);
        }

        public async Task Divorce(SocketCommandContext Context, SocketGuildUser user)
        {
            if (user.Id == Context.User.Id)
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: Can't divorce yourself :eyes:");
                return;
            }

            List<MarryData> marryData = new List<MarryData>();
            if (_marryDict.ContainsKey(Context.User.Id))
            {
                _marryDict.TryGetValue(Context.User.Id, out marryData);

                if (marryData == null || marryData.Count == 0)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You are not married to anyone :frowning:");
                    return;
                }

                var foundUser = marryData.FirstOrDefault(x => x.UserId == user.Id);
                if (foundUser != null)
                {
                    marryData.Remove(foundUser);

                    if (marryData.Count == 0)
                        _marryDict.TryRemove(Context.User.Id, out marryData);
                    else
                        _marryDict.AddOrUpdate(Context.User.Id, marryData, (key, oldValue) => marryData);
                    
                    
                    
                    marryData = new List<MarryData>();
                    if (_marryDict.ContainsKey(user.Id))
                        _marryDict.TryGetValue(user.Id, out marryData);

                    var founduser = marryData.FirstOrDefault(x => x.UserId == Context.User.Id);
                    if (marryData.Contains(founduser))
                        marryData.Remove(founduser);
                    
                    if (marryData.Count == 0)
                        _marryDict.TryRemove(user.Id, out marryData);
                    else
                        _marryDict.AddOrUpdate(user.Id, marryData, (key, oldValue) => marryData);
                    
                    await Context.Channel.SendMessageAsync("You are now divorced :broken_heart:");
                    _marryDB.SaveMarryData(_marryDict);
                    return;
                }
                else
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You are not married to that person");
                    return;
                }

            }
            else
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You are not married to anyone :frowning:");
            }
        }

        public async Task ShowMarriages(SocketCommandContext Context, SocketUser user)
        {
            
            List<MarryData> marryData = GetMerryData(user);

            if (marryData == null || marryData.Count ==0)
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You are not married to anyone :frowning:");
                return;
            }
            
            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247),
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Title = $"Marriages of {user.Username}#{user.Discriminator}",
                Description = "\n",
                ThumbnailUrl = user.GetAvatarUrl()
            };

            foreach (var marry in marryData)
            {
                var userT = Context.Client.GetUser(marry.UserId);
                eb.AddField((x) =>
                {
                    x.Name = (userT == null
                        ? $"Couldn't find Name ({marry.UserId})"
                        : $"{userT.Username}#{userT.Discriminator}");
                    x.IsInline = true;
                    x.Value =
                        $"*Married since {marry.MarriedSince.ToString().Remove(marry.MarriedSince.ToString().Length - 9)}*";
                });
            }

            await Context.Channel.SendMessageAsync("", embed: eb);
        }

        public List<MarryData> GetMerryData(SocketUser user)
        {
            if (!_marryDict.ContainsKey(user.Id))
                return null;
            List<MarryData> marryData = new List<MarryData>();
            _marryDict.TryGetValue(user.Id, out marryData);
            return marryData;
        }
    }
}

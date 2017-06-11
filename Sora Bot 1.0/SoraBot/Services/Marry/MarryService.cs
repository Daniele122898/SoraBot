using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;

namespace Sora_Bot_1.SoraBot.Services.Marry
{
    public class MarryService
    {
        //                          User, list<waifus>
        private ConcurrentDictionary<ulong, List<ulong>> _marryDict = new ConcurrentDictionary<ulong, List<ulong>>();


        public async Task Marry(SocketCommandContext Context, InteractiveService interactive, SocketGuildUser user)
        {
            List<ulong> marryPartners = new List<ulong>();
            if (_marryDict.ContainsKey(Context.User.Id))
            {
                _marryDict.TryGetValue(Context.User.Id, out marryPartners);
                if (marryPartners != null && marryPartners.Count > 0)
                {
                    if (marryPartners.Contains(user.Id))
                    {
                        await Context.Channel.SendMessageAsync(":no_entry_sign: You cannot marry someone twice!");
                        return;
                    }
                }
            }
            await Context.Channel.SendMessageAsync($"{user.Mention}, do you want to marry {Context.User.Mention}? :ring:");
            var response = await interactive.WaitForMessage(user, Context.Channel, TimeSpan.FromSeconds(20));
            if (response == null || !response.Content.Equals("yes"))
            {
                await Context.Channel.SendMessageAsync($"{user.Username} has not answered with a Yes :frowning:");
                return;
            }
            marryPartners.Add(user.Id);
            _marryDict.AddOrUpdate(Context.User.Id, marryPartners, (key, oldValue) => marryPartners);
            marryPartners = new List<ulong>();
            if (_marryDict.ContainsKey(user.Id))
                _marryDict.TryGetValue(user.Id, out marryPartners);

            marryPartners.Add(Context.User.Id);
            _marryDict.AddOrUpdate(user.Id, marryPartners, (key, oldValue) => marryPartners);
            //TODO SAVE DB
        }

    }
}

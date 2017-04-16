using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;

namespace Sora_Bot_1.SoraBot.Services
{
    public class ReminderService
    {
        /*
        private static ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<string, int>>>
            guildDict =
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<string, int>>>();
            //guild,<user,<msg,time>>


        public async Task SetReminder(SocketCommandContext Context, string message)
        {
            bool done = await Timer(Context,5000);
            //if (done)
            //{
            //    await Context.Channel.SendMessageAsync(Context.Message.ToString());
            //}
        }

        public async Task<bool> Timer(SocketCommandContext Context,int time)
        {
            bool done = false;
            await Task.Delay(time);
            done = true;
            await Context.Channel.SendMessageAsync(Context.Message.Content);
            return done;
        }*/

    }

}

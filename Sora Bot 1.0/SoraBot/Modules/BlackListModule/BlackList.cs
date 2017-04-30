using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Services.UserBlacklist;

namespace Sora_Bot_1.SoraBot.Modules.BlackListModule
{
    public class BlackList : ModuleBase<SocketCommandContext>
    {
        private BlackListService _service;

        public BlackList(BlackListService _ser)
        {
            _service = _ser;
        }

        [Command("blacklist"),
         Summary("Blacklists the specified user preventing him from using Sora any further in the guild")]
        public async Task BlackListUser([Summary("User to blacklist")] SocketGuildUser user)
        {
            await _service.BlackListUser(Context, user);
        }

        [Command("rmblacklist"),
         Summary("Removes the specified user from the blacklist allowing him to use Sora again in the guild")]
        public async Task RemoveBlackListUser([Summary("User to remove from blacklist")] SocketGuildUser user)
        {
            await _service.RemoveBlackListUser(Context, user);
        }

        [Command("showblacklist"),
         Summary("Shows all the blacklisted users in the guild")]
        public async Task ShowBlackList()
        {
            await _service.ShowBlackListedUsers(Context);
        }

    }
}

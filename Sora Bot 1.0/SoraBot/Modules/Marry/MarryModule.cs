using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Services.Marry;

namespace Sora_Bot_1.SoraBot.Modules.Marry
{
    public class MarryModule : ModuleBase<SocketCommandContext>
    {
        private InteractiveService _interactive;
        private MarryService _marryService;
        public MarryModule(MarryService marrySer, InteractiveService interactive)
        {
            _marryService = marrySer;
            _interactive = interactive;
        }

        [Command("marry", RunMode = RunMode.Async), Summary("Will ask the specified user to marry")]
        public async Task MarryUser([Summary("User to Marry")] SocketGuildUser user)
        {
            await _marryService.Marry(Context, _interactive, user);
        }

        [Command("divorce"), Summary("Will divorce from user")]
        public async Task DivorceUser([Summary("User to Divorce")] SocketGuildUser user)
        {
            await _marryService.Divorce(Context, user);
        }

        [Command("marriages"), Summary("Shows your Marriages")]
        public async Task ShowMarriages([Summary("User to check marriages of")]SocketUser user = null)
        {
            SocketUser userInfo = user ?? Context.User;
            await _marryService.ShowMarriages(Context, userInfo);
        }

    }
}

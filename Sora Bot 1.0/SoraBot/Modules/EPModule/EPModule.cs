using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Services.EPService;

namespace Sora_Bot_1.SoraBot.Modules.EPModule
{
    [Group("p")]
    public class EPModule : ModuleBase
    {
        private EPService epService;

        public EPModule(EPService ep)
        {
            epService = ep;
        }

        [Command(""), Summary("Displays short profile image of User, if not specified it will show yours")]
        public async Task SendProfile([Summary("User to show the picture of, if none given will show your own!")]IUser user = null)
        {
            var typing = Context.Channel.EnterTypingState(null);
            var userInfo = user ?? Context.User;
            await epService.ShowProfile(Context, userInfo);
            typing.Dispose();
        }

        [Command("subscribe"), Summary("Toggles the lvlup Notifier")]
        [Alias("sub")]
        public async Task ToggleEP()
        {
            await epService.ToggleEPSubscribe(Context);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Services.PatService;

namespace Sora_Bot_1.SoraBot.Modules.PatModule
{
    public class PatModule : ModuleBase
    {
        private PatService patService;


        public PatModule(PatService _patService)
        {
            patService = _patService;
        }

        [Command("pat"), Summary("Pats the person specified")]
        public async Task Pat([Summary("Person to pat")] IUser user)
        {
            await patService.AddPat(user);
            await ReplyAsync($"{Context.User.Mention} pats {user.Mention} ｡◕ ‿ ◕｡ \n http://i.imgur.com/bDMMk0L.gif");
        }

        [Command("patcount"), Summary("How many pats did this User Receive (Global Number)")]
        public async Task PatCount(
            [Summary("Person to get Patcount. If not specified it will give your own")] IUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser; // ?? if not null return left. if null return right
            await patService.CheckPats(userInfo, Context);
        }

    }
}

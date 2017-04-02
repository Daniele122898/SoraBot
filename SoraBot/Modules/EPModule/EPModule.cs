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

        [Command("top10", RunMode = RunMode.Async),
         Summary(
             "Posts the top 10 list of users sorted by EP => The EP is globaly aquired on all Guilds that Sora is on!")]
        [Alias("top", "leaderboard")]
        public async Task Top10List()
        {
            await epService.shotTop10(Context);
        }

        [Command("", RunMode = RunMode.Async), Summary("Displays short profile image of User, if not specified it will show yours")]
        public async Task SendProfile(
            [Summary("User to show the picture of, if none given will show your own!")] IUser user = null)
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

        [Command("setCord")]
        [RequireOwner]
        public async Task setCords([Remainder] string cords)
        {
            await epService.changeProfileCord(cords, Context);
        }

        [Command("setSize")]
        [RequireOwner]
        public async Task setSize(string size)
        {
            await epService.size(size, Context);
        }

        [Command("setbg", RunMode = RunMode.Async), Summary("Set's your profile BG with the provided URL. If no URL is specified it will return to the default Profile Crad. **this feature requires lvl 20!**")]
        public async Task setBG(string url = null)
        {
            await epService.SetBG(url, Context);
        }
    }
}
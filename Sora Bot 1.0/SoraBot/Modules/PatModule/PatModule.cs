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

        private string[] _hugs = new string[]
        {
            "https://media.giphy.com/media/od5H3PmEG5EVq/giphy.gif",
            "https://68.media.tumblr.com/8d7f21698a2e2c85bf9ff7a829488336/tumblr_nmrmhleuYw1u4zujko1_500.gif",
            "https://m.popkey.co/fca5d5/bXDgV.gif",
            "https://media.giphy.com/media/143v0Z4767T15e/giphy.gif",
            "https://68.media.tumblr.com/21f89b12419bda49ce8ee33d50f01f85/tumblr_o5u9l1rBqg1ttmhcxo1_500.gif",
            "https://myanimelist.cdn-dena.com/s/common/uploaded_files/1461073447-335af6bf0909c799149e1596b7170475.gif",
            "https://myanimelist.cdn-dena.com/s/common/uploaded_files/1460988091-6e86cd666a30fcc1128c585c82a20cdd.gif",
            "https://media.giphy.com/media/du8yT5dStTeMg/giphy.gif",
            "https://media.giphy.com/media/kvKFM3UWg2P04/giphy.gif",
            "https://media.giphy.com/media/wnsgren9NtITS/giphy.gif",
            "http://media.giphy.com/media/veU1qeC9SJiKY/giphy.gif"
        };

        private string[] _pokes = new string[]
        {
            "https://media.giphy.com/media/ovbDDmY4Kphtu/giphy.gif",
            "https://s-media-cache-ak0.pinimg.com/originals/e5/bd/ea/e5bdea33daa43791fb17f8575c059779.gif",
            "https://media.giphy.com/media/pWd3gD577gOqs/giphy.gif",
            "https://media.giphy.com/media/WvVzZ9mCyMjsc/giphy.gif",
            "https://media.giphy.com/media/LXTQN2kRbaqAw/giphy.gif",
            "https://lh6.googleusercontent.com/-Rc6igf9sWvk/U1PkqRGRsiI/AAAAAAAAAPE/5WvzL636Cl4/w500-h281/13854_1235188662.gif",
            "http://i.imgur.com/VtWJ8ak.gif",
            "https://31.media.tumblr.com/7c8457fd628f55b768ac2c6232a893cf/tumblr_mnycv2sm2f1r43mgoo1_500.gif"
        };

        [Command("pat"), Summary("Pats the person specified")]
        public async Task Pat([Summary("Person to pat")] IUser user)
        {
            if (Context.User.Id == user.Id)
            {
                await ReplyAsync($"{Context.User.Mention} Why are you patting yourself.. Are you okay? ｡ﾟ･（>﹏<）･ﾟ｡ \n https://media.giphy.com/media/wUArrd4mE3pyU/giphy.gif");
                return;
            }
            await patService.AddPat(user, Context);
            await ReplyAsync($"{Context.User.Mention} pats {user.Mention} ｡◕ ‿ ◕｡ \n http://i.imgur.com/bDMMk0L.gif");
        }

        [Command("patcount"), Summary("How many pats did this User Receive (Global Number)")]
        public async Task PatCount(
            [Summary("Person to get Patcount. If not specified it will give your own")] IUser user = null)
        {
            var userInfo = user ?? Context.User; // ?? if not null return left. if null return right
            await patService.CheckPats(userInfo, Context);
        }

        [Command("hug"), Summary("Hugs the specified person")]
        public async Task Hug([Summary("Person to hug")]IUser user)
        {
            if(Context.User.Id == user.Id)
            {
                await ReplyAsync($"{Context.User.Mention} don't hug yourself ;-; At least take this pillow (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ \n http://i.imgur.com/CM0of.gif");
                return;
            }
            var r = new Random();
            await ReplyAsync($"{Context.User.Mention} hugged {user.Mention} °˖✧◝(⁰▿⁰)◜✧˖°\n{_hugs[r.Next(0,_hugs.Length-1)]}");
        }

        [Command("poke"), Summary("Pokes the specified person")]
        public async Task Poke([Summary("Person to poke")]IUser user)
        {
            var r = new Random();
            await ReplyAsync($"{Context.User.Mention} poked {user.Mention} ( ≧Д≦)\n{_pokes[r.Next(0, _pokes.Length - 1)]}");
        }


    }
}

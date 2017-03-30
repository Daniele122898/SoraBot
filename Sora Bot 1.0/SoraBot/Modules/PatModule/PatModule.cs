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

        private string[] _pats = new string[]
        {
            "https://media.giphy.com/media/3ohzdLjvu2Q8rQLspq/source.gif",
            "http://i.imgur.com/bDMMk0L.gif",
            "http://pa1.narvii.com/5673/aa76b9a3ebc376626c6dc748a5983dffcf364277_hq.gif",
            "http://i.imgur.com/gQ5r1li.gif",
            "http://funnypictures1.fjcdn.com/funny_gifs/Head_e12a8e_6102763.gif",
            "http://i.imgur.com/M5kqhq9.gif",
            "https://s-media-cache-ak0.pinimg.com/originals/c0/3f/58/c03f5817acde4b1c168d31ffe02c522e.gif",
            "https://68.media.tumblr.com/2d61aa2fd9286f5670fbb17b6e56475f/tumblr_o4ufimpBNt1tydz8to1_500.gif",
            "https://68.media.tumblr.com/cf71997201ee0463db3be5445eaa8743/tumblr_oij2qrsdIO1vwt3qvo1_500.gif",
            "http://pa1.narvii.com/5983/85777dd28aa87072ee5a9ed759ab0170b3c60992_hq.gif",
            "https://m.popkey.co/a5cfaf/1x6lW.gif",
            "http://funnypictures1.fjcdn.com/funny_gifs/Head_389b42_6102763.gif",
            "https://media.giphy.com/media/ye7OTQgwmVuVy/giphy.gif",
            "https://33.media.tumblr.com/cb4da84b16d8e189c5b7a61632a54953/tumblr_nrcwmt2SNG1r4vymlo1_400.gif",
            "https://media.giphy.com/media/KZQlfylo73AMU/giphy.gif",
            "http://i0.kym-cdn.com/photos/images/original/000/915/038/7e9.gif",
            "https://68.media.tumblr.com/f746c0d35113e5d5bc855521d44ebca9/tumblr_nqzi8y6VkB1uo77uno1_540.gif"
        };

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

        private string[] _slaps = new string[]
        {
            "https://i.imgur.com/oY3UC4g.gif",
            "https://49.media.tumblr.com/bff05c1c0d1cb19a35495823b6257cda/tumblr_nhdyy5EhyG1u1toifo5_400.gif",
            "http://i1280.photobucket.com/albums/a489/Lilyfied/Anime%20Gif/slapgif_zps5164a18e.gif",
            "https://media.giphy.com/media/jLeyZWgtwgr2U/giphy.gif",
            "https://s-media-cache-ak0.pinimg.com/originals/65/57/f6/6557f684d6ffcd3cd4558f695c6d8956.gif",
            "http://i0.kym-cdn.com/photos/images/newsfeed/000/940/326/086.gif",
            "https://s-media-cache-ak0.pinimg.com/originals/4e/9e/a1/4e9ea150354ad3159339b202cbc6cad9.gif",
            "https://cdn.discordapp.com/attachments/286476586952425472/296394039878615040/giphy.gif",
            "https://cdn.discordapp.com/attachments/286476586952425472/296394610832572426/giphy-3.gif",
            "http://i.imgur.com/3rHE4Ee.gif",
            "http://i.imgur.com/ihkVAis.gif"
        };

        [Command("pat"), Summary("Pats the person specified")]
        public async Task Pat([Summary("Person to pat")] IUser user)
        {
            if (Context.User.Id == user.Id)
            {
                await ReplyAsync($"{Context.User.Mention} Why are you patting yourself.. Are you okay? ｡ﾟ･（>﹏<）･ﾟ｡ \n https://media.giphy.com/media/wUArrd4mE3pyU/giphy.gif");
                return;
            }
            var r = new Random();
            await patService.AddPat(user, Context);
            await ReplyAsync($"{Context.User.Mention} pats {user.Mention} ｡◕ ‿ ◕｡ \n {_pats[r.Next(0,_pats.Length-1)]}");
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

        [Command("slap"), Summary("Slaps the specified person <.<")]
        public async Task Slap([Summary("Person to slap")]IUser user)
        {
            var r = new Random();
            if (Context.User.Id == user.Id)
            {
                await ReplyAsync($"{Context.User.Mention} why would you slap yourself... Are you okay? 〣( ºΔº )〣\n https://media.giphy.com/media/Okk9cb1dvtMxq/giphy.gif");
                return;
            }
            await ReplyAsync($"{Context.User.Mention} slapped {user.Mention} (ᗒᗩᗕ)՞ \n{_slaps[r.Next(0, _slaps.Length - 1)]}");
        }


    }
}

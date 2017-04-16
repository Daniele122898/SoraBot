using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Sora_Bot_1.SoraBot.Services
{
    public class UbService
    {
        public async Task GetUbDef(SocketCommandContext Context, string urban)
        {
            try
            {
                var vc = new HttpClient();
                var search = System.Net.WebUtility.UrlEncode(urban);
                string req = await vc.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + search);
                var ub = JsonConvert.DeserializeObject<UbContainer>(req);

                var eb = ub.GetEmbed();
                eb.WithFooter(x => {
                    x.Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                });
                eb.Build();
                

                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find Urban Dictionary entry. Try later or try another one.");
            }
        }
    }

    public class UbContainer
    {
        public string[] tags;
        public string result_type;
        public UbDef[] list;
        public string[] sounds;
        public EmbedBuilder GetEmbed() =>
            new EmbedBuilder()
            .WithColor(new Color(4, 97, 247))
            .WithAuthor(x => { x.Name = "Urban Dictionary"; x.IconUrl = "https://lh5.ggpht.com/oJ67p2f1o35dzQQ9fVMdGRtA7jKQdxUFSQ7vYstyqTp-Xh-H5BAN4T5_abmev3kz55GH=w300"; })
            .WithTitle($"Definition of {list[0].word}")
            .WithDescription(list[0].definition)
            .WithUrl(list[0].permalink)
            .AddField(x => x.WithName("Examples").WithValue(list[0].example).WithIsInline(false))
            .AddField(x => x.WithName("Author").WithValue(list[0].author).WithIsInline(true))
            .AddField(x => x.WithName("Stats").WithValue($"{list[0].thumbs_up} :thumbsup:\t{list[0].thumbs_down} :thumbsdown:").WithIsInline(true))
            .AddField(x => x.WithName("Tags").WithValue(GetTags()).WithIsInline(false));
        //.Build(); .AddField(x => x.WithName($"Definition of {list[0].word}").WithValue(list[0].definition).WithIsInline(false)).WithUrl(list[0].permalink)

        public string GetTags()
        {
            string combinedTags = "";
            foreach (var t in tags)
            {
                combinedTags += $"{t}, ";
            }
            return combinedTags;
        }
    }

    public class UbDef
    {
        public string definition { get; set; }
        public string example { get; set; }
        public string word { get; set; }
        public string author { get; set; }
        public string thumbs_up { get; set; }
        public string thumbs_down { get; set; }
        public string permalink { get; set; }
    }
}

using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Sora_Bot_1.SoraBot.Services
{
    public class UbService
    {
        public async Task GetUbDef(CommandContext Context, string urban)
        {
            try
            {
                var eb = new EmbedBuilder();

                var vc = new HttpClient();

                eb.WithAuthor(x =>
                {
                    x.Name = "Urban Dictionary";
                    x.WithIconUrl("https://lh5.ggpht.com/oJ67p2f1o35dzQQ9fVMdGRtA7jKQdxUFSQ7vYstyqTp-Xh-H5BAN4T5_abmev3kz55GH=w300");
                });
                string req = await vc.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + urban);
                eb.WithColor(new Color(4, 97, 247));
                MatchCollection col = Regex.Matches(req, @"(?<=definition"":"")[ -z~-🧀]+(?="",""permalink)");
                MatchCollection col2 = Regex.Matches(req, @"(?<=example"":"")[ -z~-🧀]+(?="",""thumbs_down)");
                if (col.Count == 0)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Nothing found :frowning:");
                    return;
                }
                Random r = new Random();
                string outpt = "Failed fetching data from Urban Dictionary, please try later!";
                string outpt2 = "No Example";
                int max = r.Next(0, col.Count);
                for (int i = 0; i <= max; i++)
                {
                    outpt = urban + "\r\n\r\n" + col[i].Value;
                }
                for (int i = 0; i <= max; i++)
                {
                    outpt2 = "\r\n\r\n" + col2[i].Value;
                }

                outpt = outpt.Replace("\\r", "\r");
                outpt = outpt.Replace("\\n", "\n");
                outpt2 = outpt2.Replace("\\r", "\r");
                outpt2 = outpt2.Replace("\\n", "\n");

                eb.AddField(x =>
                {
                    x.Name = "Definition";
                    x.Value = outpt;
                });

                eb.AddField(x =>
                {
                    x.Name = "Examples";
                    x.Value = outpt2;
                });

                eb.WithFooter(x =>
                {
                    x.Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                    x.Build();
                });

                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }
    }
}

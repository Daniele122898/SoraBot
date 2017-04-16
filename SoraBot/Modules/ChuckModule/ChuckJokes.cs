using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace Sora_Bot_1.SoraBot.Modules.ChuckModule
{
    public class ChuckJokes : ModuleBase<SocketCommandContext>
    {
        [Command("chucknorris", RunMode = RunMode.Async), Alias("chuck", "norris"), Summary("Posts a random chuck norris joke")]
        public async Task GetChuckNorris()
        {
            using (var http = new HttpClient())
            {
                string response = await http.GetStringAsync("https://api.chucknorris.io/jokes/random").ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<NorrisData>(response);
                await Context.Channel.SendMessageAsync("", embed: data.GetEmbed());
            }
        }
    }

    public class NorrisData
    {
        public string icon_url { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string value { get; set; }

        public EmbedBuilder GetEmbed() =>
            new EmbedBuilder()
            .WithColor(new Color(4, 97, 247))
            .WithAuthor(x => { x.Name = "Chuck Norris"; x.IconUrl = $"{icon_url}"; })
            .WithUrl($"{url}")
            .WithDescription($"{value}");
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace Sora_Bot_1.SoraBot.Services
{
    public class ImdbService
    {
        public async Task GetImdb(CommandContext Context, string target)
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();

                var movie = await OmdbProvider.FindMovie(target);
                if(movie == null)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find movie/series");
                    return;
                }
                else
                {
                    await Context.Channel.SendMessageAsync("", false, movie.GetEmbed());
                }
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find movie/series");
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }
    }

    public static class OmdbProvider
    {
        private const string queryUrl = "http://www.omdbapi.com/?t={0}&y=&plot=full&r=json";

        public static async Task<OmdbMovie> FindMovie(string name)
        {
            using (var http = new HttpClient())
            {
                var res = await http.GetStringAsync(String.Format(queryUrl, name.Trim().Replace(' ', '+'))).ConfigureAwait(false);
                var movie = JsonConvert.DeserializeObject<OmdbMovie>(res);

                return movie;
            }
        }
    }

    public class OmdbMovie
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Released { get; set; }
        public string ImdbRating { get; set; }
        public string ImdbId { get; set; }
        public string Genre { get; set; }
        public string Plot { get; set; }
        public string Poster { get; set; }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string type { get; set; }
        public Embed GetEmbed() =>
            new EmbedBuilder()
            .WithColor(new Color(4, 97, 247))
            .WithAuthor(x => { x.Name = "IMDb"; x.IconUrl = "http://image.prntscr.com/image/6fdc466f14524542afbd3f923a4595ee.png"; })
            .WithTitle(Title)
            .WithUrl($"http://www.imdb.com/title/{ImdbId}/")
            .WithDescription(Plot)
            .AddField(x => x.WithName("Rating").WithValue(ImdbRating).WithIsInline(true))
            .AddField(x => x.WithName("Genre").WithValue(Genre).WithIsInline(true))
            .AddField(x => x.WithName("Released").WithValue(Released).WithIsInline(true))
            .AddField(x => x.WithName("Director").WithValue(Director).WithIsInline(true))
            .AddField(x => x.WithName("Writer").WithValue(Writer).WithIsInline(true))
            .AddField(x => x.WithName("Type").WithValue(type).WithIsInline(true))
            .WithThumbnailUrl(Poster)
            .Build();
    }
}

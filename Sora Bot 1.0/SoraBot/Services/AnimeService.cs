using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using Discord.Addons.InteractiveCommands;

namespace Sora_Bot_1.SoraBot.Services
{
    public class AnimeService
    {

        public const string apiUrl = "https://anilist.co/api/";
        private string anilistToken;
        private int timeToUpdate;
        private string clientId = "";
        private string clientSecret = "";

        public AnimeService()
        {
            var configDict = ConfigService.ConfigService.getConfig();
            if (!configDict.TryGetValue("client_id", out clientId))
            {
                Console.WriteLine("COULDN'T GET CLIENT ID FROM CONFIG!");
            }
            if (!configDict.TryGetValue("client_secret", out clientSecret))
            {
                Console.WriteLine("COULDN'T GET CLIENT SECRET FROM CONFIG!");
            }
            RequestAuth();
        }

        public async Task RequestAuth()
        {
            try
            {
                var headers = new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", clientId},
                    {"client_secret", clientSecret},
                };

                using (var http = new HttpClient())
                {
                    //http.AddFakeHeaders();
                    http.DefaultRequestHeaders.Clear();
                    var formContent = new FormUrlEncodedContent(headers);
                    var response = await http.PostAsync("https://anilist.co/api/auth/access_token", formContent).ConfigureAwait(false);
                    var stringContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    anilistToken = JObject.Parse(stringContent)["access_token"].ToString();
                }
                timeToUpdate = Environment.TickCount + 1700000;
                Console.WriteLine("ANILIST AUTHENTICATION SUCCESSFULL");
                await SentryService.SendMessage("ANILIST AUTHENTICATION **SUCCESSFULL**");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendMessage("ANILIST AUTHENTICATION **FAILED**");
                await SentryService.SendError(e);
            }
           
        }

        public async Task GetManga(SocketCommandContext Context, string manga, InteractiveService interactive)
        {
            try
            {
                if (Environment.TickCount > timeToUpdate)
                {
                    await RequestAuth();
                }
                var search = System.Net.WebUtility.UrlEncode(manga);
                var link = "http://anilist.co/api/manga/search/" + Uri.EscapeUriString(search);
                using (var http = new HttpClient())
                {
                    var res = await http.GetStringAsync(link + $"?access_token={anilistToken}").ConfigureAwait(false);
                    var results = JArray.Parse(res);
                    int index;
                    if (results.Count > 1)
                    {
                        string choose = "";
                        var ebC = new EmbedBuilder()
                        {
                            Color = new Color(4, 97, 247),
                            Title = "Enter the Index of the Manga you want more info about.",
                        };
                        int count = 1;
                        foreach (var r in results)
                        {
                            choose += $"**{count}.** {r["title_english"]}\n";
                            count++;
                        }
                        ebC.Description = choose;
                        await Context.Channel.SendMessageAsync("", embed: ebC);
                        var response = await interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(20));
                        if (response == null)
                        {
                            await Context.Channel.SendMessageAsync($":no_entry_sign: Answer timed out {Context.User.Mention} (≧д≦ヾ)");
                            return;
                        }
                        if (!Int32.TryParse(response.Content, out index))
                        {
                            await Context.Channel.SendMessageAsync(":no_entry_sign: Only add the Index");
                            return;
                        }
                        if (index > (results.Count) || index < 1)
                        {
                            await Context.Channel.SendMessageAsync(":no_entry_sign: Invalid Number");
                            return;
                        }
                    }
                    else
                    {
                        index = 1;
                    }
                    var smallObj = JArray.Parse(res)[index-1];
                    var manData = await http.GetStringAsync("http://anilist.co/api/manga/" + smallObj["id"] + $"?access_token={anilistToken}").ConfigureAwait(false);
                    //await Context.Channel.SendMessageAsync(manData);
                    //return await Task.Run(() => { try { return JsonConvert.DeserializeObject<AnimeResult>(aniData); } catch { return null; } }).ConfigureAwait(false);
                    var mangaDa = JsonConvert.DeserializeObject<MangaResult>(manData);

                    var eb = mangaDa.GetEmbed();
                    eb.WithFooter(x =>
                    {
                        x.Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    });
                    eb.Build();

                    await Context.Channel.SendMessageAsync("", false, eb);
                }

            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find Manga. Try later or try another one.");
            }
        }

        public async Task GetAnimeChar(SocketCommandContext Context, string charName, InteractiveService interactive)
        {
            try
            {
                if (Environment.TickCount > timeToUpdate)
                {
                    await RequestAuth();
                }

                var search = System.Net.WebUtility.UrlEncode(charName);
                var link = "http://anilist.co/api/character/search/" + Uri.EscapeUriString(search);
                using (var http = new HttpClient())
                {
                    var res = await http.GetStringAsync(link + $"?access_token={anilistToken}").ConfigureAwait(false);
                    var results = JArray.Parse(res);
                    int index;
                    if (results.Count > 1)
                    {

                        string choose = "";
                        var ebC = new EmbedBuilder()
                        {
                            Color = new Color(4, 97, 247),
                            Title = "Enter the Index of the Character you want more info about.",
                        };
                        int count = 1;
                        foreach (var r in results)
                        {
                            choose += $"**{count}.** {r["name_first"]} {r["name_last"]}\n";
                            count++;
                        }
                        ebC.Description = choose;
                        await Context.Channel.SendMessageAsync("", embed: ebC);
                        var response = await interactive.WaitForMessage(Context.User, Context.Channel,
                            TimeSpan.FromSeconds(20));
                        if (response == null)
                        {
                            await Context.Channel.SendMessageAsync(
                                $":no_entry_sign: Answer timed out {Context.User.Mention} (≧д≦ヾ)");
                            return;
                        }

                        if (!Int32.TryParse(response.Content, out index))
                        {
                            await Context.Channel.SendMessageAsync(":no_entry_sign: Only add the Index");
                            return;
                        }
                        if (index > (results.Count) || index < 1)
                        {
                            await Context.Channel.SendMessageAsync(":no_entry_sign: Invalid Number");
                            return;
                        }
                    }
                    else
                    {
                        index = 1;
                    }
                    var smallObj = JArray.Parse(res)[index - 1];
                    var aniData =
                        await http.GetStringAsync("http://anilist.co/api/character/" + smallObj["id"] +
                                                  $"?access_token={anilistToken}").ConfigureAwait(false);
                    //return await Task.Run(() => { try { return JsonConvert.DeserializeObject<AnimeResult>(aniData); } catch { return null; } }).ConfigureAwait(false);
                    var animeDa = JsonConvert.DeserializeObject<CharacterResult>(aniData);

                    var eb = animeDa.GetEmbed();
                    eb.WithFooter(x =>
                    {
                        x.Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    });

                    //.AddField(efb => efb.WithName("Japanese Name").WithValue(name_japanese).WithIsInline(true));
                    if (!String.IsNullOrWhiteSpace(animeDa.name_japanese))
                    {
                        eb.AddField((x) =>
                        {
                            x.Name = "Japanese Name";
                            x.IsInline = true;
                            x.Value = animeDa.name_japanese;
                        });
                    }

                    if (!String.IsNullOrWhiteSpace(animeDa.name_alt))
                    {
                        eb.AddField((x) =>
                        {
                            x.Name = "Alt Name";
                            x.IsInline = true;
                            x.Value = animeDa.name_alt;
                        });
                    }


                    

                    eb.Build();

                    await Context.Channel.SendMessageAsync("", false, eb);
                }
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find Character. Try later or try another one.");
            }
        }

        public async Task GetAnime(SocketCommandContext Context, string anime, InteractiveService interactive)
        {
            try
            {
                if(Environment.TickCount > timeToUpdate)
                {
                    await RequestAuth();
                }
                var search = System.Net.WebUtility.UrlEncode(anime);
                var link = "http://anilist.co/api/anime/search/" + Uri.EscapeUriString(search);
                using (var http = new HttpClient())
                {
                    var res = await http.GetStringAsync(link + $"?access_token={anilistToken}").ConfigureAwait(false);
                    var results = JArray.Parse(res);
                    int index;
                    if (results.Count > 1)
                    {

                        string choose = "";
                        var ebC = new EmbedBuilder()
                        {
                            Color = new Color(4, 97, 247),
                            Title = "Enter the Index of the Anime you want more info about.",
                        };
                        int count = 1;
                        foreach (var r in results)
                        {
                            choose += $"**{count}.** {r["title_english"]}\n";
                            count++;
                        }
                        ebC.Description = choose;
                        await Context.Channel.SendMessageAsync("", embed: ebC);
                        var response = await interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(20));
                        if (response == null)
                        {
                            await Context.Channel.SendMessageAsync($":no_entry_sign: Answer timed out {Context.User.Mention} (≧д≦ヾ)");
                            return;
                        }
                        
                        if (!Int32.TryParse(response.Content, out index))
                        {
                            await Context.Channel.SendMessageAsync(":no_entry_sign: Only add the Index");
                            return;
                        }
                        if (index > (results.Count) || index < 1)
                        {
                            await Context.Channel.SendMessageAsync(":no_entry_sign: Invalid Number");
                            return;
                        }
                    }
                    else
                    {
                        index = 1;
                    }
                    var smallObj = JArray.Parse(res)[index-1];
                    var aniData = await http.GetStringAsync("http://anilist.co/api/anime/" + smallObj["id"] + $"?access_token={anilistToken}").ConfigureAwait(false);
                    //await Context.Channel.SendMessageAsync(aniData);
                    //return await Task.Run(() => { try { return JsonConvert.DeserializeObject<AnimeResult>(aniData); } catch { return null; } }).ConfigureAwait(false);
                    var animeDa = JsonConvert.DeserializeObject<AnimeResult>(aniData);

                    var eb = animeDa.GetEmbed();
                    eb.WithFooter(x => {
                        x.Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    });
                    eb.Build();

                    await Context.Channel.SendMessageAsync("", false, eb);
                }

            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find Anime. Try later or try another one.");
            }   
        }

    }

    public class CharacterResult
    {
        public int id { get; set; }
        public string name_first { get; set; }
        public string name_last { get; set; }
        public string name_japanese { get; set; }
        public string name_alt { get; set; }
        public string info { get; set; }
        public bool favorite { get; set; }
        public string image_url_lge { get; set; }
        public string image_url_med { get; set; }
        public string Link => "http://anilist.co/character/" + id;
        public string Synopsis => info?.Substring(0, info.Length > 2000 ? 2000 : info.Length) + (info.Length > 2000 ? "..." : "");

        public EmbedBuilder GetEmbed() =>
            new EmbedBuilder()
                .WithColor(new Color(4, 97, 247))
                .WithAuthor(x =>
                {
                    x.Name = "Anilist";
                    x.IconUrl = "https://anilist.co/img/logo_al.png";
                })
                .WithTitle($"{name_first} {name_last}")
                .WithUrl(Link)
                .WithDescription($"{(String.IsNullOrWhiteSpace(Synopsis)? "No Info found!": "")}" + Synopsis.Replace("<br>", Environment.NewLine))
                .WithImageUrl(image_url_lge);

    }

    public class MangaResult
    {
        public int id;
        public string publishing_status;
        public string image_url_lge;
        public string title_english;
        public int total_chapters;
        public int total_volumes;
        public string description;
        public string start_date;
        public string end_date;
        public string[] Genres;
        public string average_score;
        public string Link => "http://anilist.co/manga/" + id;
        public string Synopsis => description?.Substring(0, description.Length > 2000 ? 2000 : description.Length) + (description.Length > 2000 ? "..." : "");
        public EmbedBuilder GetEmbed() =>
            new EmbedBuilder()
            .WithColor(new Color(4, 97, 247))
            .WithAuthor(x => { x.Name = "Anilist"; x.IconUrl = "https://anilist.co/img/logo_al.png"; })
            .WithTitle(title_english)
            .WithUrl(Link)
            .WithDescription(Synopsis.Replace("<br>", Environment.NewLine))
            .WithImageUrl(image_url_lge)
            .AddField(efb => efb.WithName("Chapters").WithValue(total_chapters.ToString()).WithIsInline(true))
            .AddField(efb => efb.WithName("Status").WithValue(publishing_status.ToString()).WithIsInline(true))
            .AddField(efb => efb.WithName("Genres").WithValue(String.Join(", ", Genres)).WithIsInline(true))
            .AddField(efb => efb.WithName("Score").WithValue(average_score + " / 100").WithIsInline(true))
            .AddField(efb => efb.WithName("Published").WithValue($"Start: {start_date.Remove(10)}\n{(String.IsNullOrWhiteSpace(end_date) ? "Ongoing" : $"End:   {end_date.Remove(10)}")}").WithIsInline(true));
    }

    public class AnimeResult
    {
        public int id;
        public string AiringStatus => airing_status.ToLowerInvariant();
        public string airing_status;
        public string title_english;
        public int total_episodes;
        public string description;
        public string image_url_lge;
        public string start_date;
        public string end_date;
        public string[] Genres;
        public string average_score;

        public string Link => "http://anilist.co/anime/" + id;
        public string Synopsis => description?.Substring(0, description.Length > 2000 ? 2000 : description.Length) + (description.Length > 2000 ? "..." : "");
        public EmbedBuilder GetEmbed() =>
            new EmbedBuilder()
            .WithColor(new Color(4, 97, 247))
            .WithAuthor(x => { x.Name = "Anilist"; x.IconUrl = "https://anilist.co/img/logo_al.png"; })
            .WithTitle(title_english)
            .WithUrl(Link)
            .WithDescription(Synopsis.Replace("<br>", Environment.NewLine))
            .WithImageUrl(image_url_lge)
            .AddField(efb => efb.WithName("Episodes").WithValue(total_episodes.ToString()).WithIsInline(true))
            .AddField(efb => efb.WithName("Status").WithValue(AiringStatus.ToString()).WithIsInline(true))
            .AddField(efb => efb.WithName("Genres").WithValue(String.Join(",", Genres)).WithIsInline(true))
            .AddField(efb => efb.WithName("Score").WithValue(average_score + " / 100").WithIsInline(true))
            .AddField(efb => efb.WithName("Aired").WithValue($"Start: {start_date.Remove(10)}\n{(String.IsNullOrWhiteSpace(end_date) ? "Ongoing": $"End:   {end_date.Remove(10)}")}").WithIsInline(true));
            //.WithThumbnailUrl(image_url_lge);
    }
}

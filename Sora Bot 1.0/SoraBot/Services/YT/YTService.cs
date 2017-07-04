using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Services.YT
{
    public class YTService
    {
        private string ytApiKey = "";
        public YTService()
        {
            var configDict = ConfigService.ConfigService.getConfig();
            if (!configDict.TryGetValue("ytapikey", out ytApiKey))
            {
                Console.WriteLine("COULDN'T GET YT API KEY FROM CONFIG!");
            }
        }

        public async Task GetYTResults(SocketCommandContext Context, string query)
        {
            try
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = ytApiKey,
                    ApplicationName = "SoraBot" //this.GetType().ToString()
                });

                var searchListRequest = youtubeService.Search.List("snippet");
                var search = System.Net.WebUtility.UrlEncode(query);
                searchListRequest.Q = search;
                searchListRequest.MaxResults = 10;

                var searchListResponse = await searchListRequest.ExecuteAsync();

                List<string> videos = new List<string>();
                List<string> channels = new List<string>();
                List<string> playlists = new List<string>();

                foreach (var searchResult in searchListResponse.Items)
                {
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":
                            videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                            break;

                        case "youtube#channel":
                            channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                            break;

                        case "youtube#playlist":
                            playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                            break;
                    }
                }

                await Context.Channel.SendMessageAsync(String.Format("Videos: \n{0}\n", String.Join("\n", videos)));
                await Context.Channel.SendMessageAsync(String.Format("Channels: \n{0}\n", String.Join("\n", channels)));
                await Context.Channel.SendMessageAsync(String.Format("Playlists: \n{0}\n", String.Join("\n", playlists)));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
            
        }

        public async Task<string> GetYtURL(SocketCommandContext Context, string name, InteractiveService interactive, Discord.Rest.RestUserMessage msg)
        {
            try
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = ytApiKey,
                    ApplicationName = "SoraBot" //this.GetType().ToString()
                });

                var searchListRequest = youtubeService.Search.List("snippet");
                var search = System.Net.WebUtility.UrlEncode(name);
                searchListRequest.Q = search;
                searchListRequest.MaxResults = 10;

                var searchListResponse = await searchListRequest.ExecuteAsync();

                List<string> videos = new List<string>();
                List<Google.Apis.YouTube.v3.Data.SearchResult> videosR = new List<Google.Apis.YouTube.v3.Data.SearchResult>();

                foreach (var searchResult in searchListResponse.Items)
                {
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":
                            videos.Add(String.Format("{0}", searchResult.Snippet.Title));
                            videosR.Add(searchResult);
                            break;
                    }
                }
                int index;
                if (videos.Count > 1)
                {
                    var eb = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247),
                        Title = "Enter the Index of the YT video you want to add.",
                    };
                    string vids = "";
                    int count = 1;
                    foreach (var v in videos)
                    {
                        vids += $"**{count}.** {v}\n";
                        count++;
                    }
                    eb.Description = vids;
                    var del = await Context.Channel.SendMessageAsync("", embed: eb);
                    var response = await interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(20));
                    await del.DeleteAsync();
                    if (response == null)
                    {
                        await msg.ModifyAsync(x =>
                        {
                            x.Content = $":no_entry_sign: Answer timed out {Context.User.Mention} (≧д≦ヾ)";
                        });
                        return "f2";
                    }
                    if (!Int32.TryParse(response.Content, out index))
                    {
                        await msg.ModifyAsync(x =>
                        {
                            x.Content = $":no_entry_sign: Only add the Index";
                        });
                        return "f2";
                    }
                    if (index > (videos.Count) || index < 1)
                    {
                        await msg.ModifyAsync(x =>
                        {
                            x.Content = $":no_entry_sign: Invalid Number";
                        });
                        return "f2";
                    }

                }
                else
                {
                    index = 1;
                }
                return $"https://www.youtube.com/watch?v={videosR[index-1].Id.VideoId}";
                //await Context.Channel.SendMessageAsync(String.Format("Videos: \n{0}\n", String.Join("\n", videos)));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
            return "f";
        }
    }
}

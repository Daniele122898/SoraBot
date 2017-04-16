using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Linq;

namespace Sora_Bot_1.SoraBot.Services.LeagueOfLegends
{
    public class lolService
    {
        private string lolapikey = "";
        public lolService()
        {
            var configDict = ConfigService.ConfigService.getConfig();
            if (!configDict.TryGetValue("lolapikey", out lolapikey))
            {
                Console.WriteLine("COULDN'T GET LOL API KEY FROM CONFIG!");
            }
        }
        
        public List<string> regions = new List<string>
        {
            "euw", "na", "eune", "jp", "kr","lan","las","oce","tr","ru"
        };

        public async Task GetUserStats(SocketCommandContext Context, string region,string name)
        {
            try
            {
                region = region.ToLower();
                if (!regions.Contains(region))
                {
                    await Context.Channel.SendMessageAsync($":no_entry_sign: The region {region} is not valid!");
                    return;
                }

                var search = System.Net.WebUtility.UrlEncode(name);
                string response = "";
                using (var http = new HttpClient())
                {
                    var user = await http.GetStringAsync($"https://{region}.api.pvp.net/api/lol/{region}/v1.4/summoner/by-name/{name}?api_key={lolapikey}").ConfigureAwait(false);

                    var data2 = JObject.Parse(user);
                    var userName = Regex.Replace(name, @"\s+", "").ToLower();

                    var id = data2[userName]["id"];
                    response = await http.GetStringAsync($"https://{region}.api.riotgames.com/api/lol/{region}/v1.3/stats/by-summoner/{id}/summary?&api_key={lolapikey}").ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<lolData>(response);

                    var eb = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247),
                        Author = new EmbedAuthorBuilder
                        {
                            Name = "League of Legends",
                            IconUrl = "https://yt3.ggpht.com/-AEerXPqHm3M/AAAAAAAAAAI/AAAAAAAAAAA/S8WpkwxItLQ/s900-c-k-no-mo-rj-c0xffffff/photo.jpg"
                        },
                        ThumbnailUrl = $"http://avatar.leagueoflegends.com/{region}/{search}.png"
                    };
                    var rankedsolo5v5 = data.playerStatSummaries.Where(x => x.playerStatSummaryType == "RankedSolo5x5").FirstOrDefault();
                    if (rankedsolo5v5 != null)
                    {
                        eb.AddField((x) =>
                        {
                            x.Name = "Ranked Solo 5v5";
                            x.IsInline = false;
                            x.Value = $"" +
                            $"**Wins / losses:** {rankedsolo5v5.wins} / {rankedsolo5v5.losses}\n" +
                            $"**Total Champion Kills:** {rankedsolo5v5.aggregatedStats.totalChampionKills}\n" +
                            $"**Total Turret Killed:** {rankedsolo5v5.aggregatedStats.totalTurretsKilled}\n" +
                            $"**Total Minion Kills:** {rankedsolo5v5.aggregatedStats.totalMinionKills}\n" +
                            $"**Total Neutral Minions Killed:** {rankedsolo5v5.aggregatedStats.totalNeutralMinionsKilled}\n" +
                            $"**Total Assits:** {rankedsolo5v5.aggregatedStats.totalAssists}";
                        });
                    }

                    var unranked = data.playerStatSummaries.Where(x => x.playerStatSummaryType == "Unranked").FirstOrDefault();
                    if (unranked != null)
                    {
                        eb.AddField((x) =>
                        {
                            x.Name = "Normal / Unranked";
                            x.IsInline = false;
                            x.Value = $"" +
                            $"**Wins:** {unranked.wins}\n" +
                            $"**Total Champion Kills:** {unranked.aggregatedStats.totalChampionKills}\n" +
                            $"**Total Turret Killed:** {unranked.aggregatedStats.totalTurretsKilled}\n" +
                            $"**Total Minion Kills:** {unranked.aggregatedStats.totalMinionKills}\n" +
                            $"**Total Neutral Minions Killed:** {unranked.aggregatedStats.totalNeutralMinionsKilled}\n" +
                            $"**Total Assits:** {unranked.aggregatedStats.totalAssists}";
                        });
                    }

                    var aram = data.playerStatSummaries.Where(x => x.playerStatSummaryType == "AramUnranked5x5").FirstOrDefault();
                    if (aram != null)
                    {
                        eb.AddField((x) =>
                        {
                            x.Name = "Aram Unranked 5v5";
                            x.IsInline = false;
                            x.Value = $"" +
                            $"**Wins:** {aram.wins}\n" +
                            $"**Total Champion Kills:** {aram.aggregatedStats.totalChampionKills}\n" +
                            $"**Total Turret Killed:** {aram.aggregatedStats.totalTurretsKilled}\n" +
                            $"**Total Assits:** {aram.aggregatedStats.totalAssists}";
                        });
                    }
                    await Context.Channel.SendMessageAsync("", embed: eb);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find stats of specified user!");
                await SentryService.SendError(e, Context);
            }
        }

        public async Task GetUser(SocketCommandContext Context, string region, string name)
        {
            try
            {
                region = region.ToLower();
                if (!regions.Contains(region))
                {
                    await Context.Channel.SendMessageAsync($":no_entry_sign: The region {region} is not valid!");
                    return;
                }
                var search = System.Net.WebUtility.UrlEncode(name);
                string response = "";
                using (var http = new HttpClient())
                {
                    response = await http.GetStringAsync($"https://{region}.api.pvp.net/api/lol/{region}/v1.4/summoner/by-name/{name}?api_key={lolapikey}").ConfigureAwait(false);
                }
                //var data = JsonConvert.DeserializeObject<LoLUser>(response);
                //TODO JsonConvert.DeserializeObject<Dictionary<string, LolUser>> <--------
                var data2 = JObject.Parse(response);
                var userName = Regex.Replace(name, @"\s+", "").ToLower();

                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "League of Legends",
                        IconUrl = "https://yt3.ggpht.com/-AEerXPqHm3M/AAAAAAAAAAI/AAAAAAAAAAA/S8WpkwxItLQ/s900-c-k-no-mo-rj-c0xffffff/photo.jpg"
                    },
                    ThumbnailUrl = $"http://avatar.leagueoflegends.com/{region}/{search}.png"
                };

                eb.AddField((x) =>
                {
                    x.Name = "Name";
                    x.IsInline = true;
                    x.Value = $"{data2[userName]["name"]}";
                });
                eb.AddField((x) =>
                {
                    x.Name = "Level";
                    x.IsInline = true;
                    x.Value = $"{data2[userName]["summonerLevel"]}";
                });
                eb.AddField((x) =>
                {
                    x.Name = "ID";
                    x.IsInline = true;
                    x.Value = $"{data2[userName]["id"]}";
                });
                //var eb = data.GetEmbed();
                //var userName = System.Net.WebUtility.UrlEncode(data.name);
                //eb.WithThumbnailUrl($"http://avatar.leagueoflegends.com/{region}/{name}.png");
                await Context.Channel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find Summoner data!");
                await SentryService.SendError(e, Context);
            }
            
        }
    }
}

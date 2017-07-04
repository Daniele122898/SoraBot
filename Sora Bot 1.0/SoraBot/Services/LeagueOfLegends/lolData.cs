using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sora_Bot_1.SoraBot.Services.LeagueOfLegends
{
    public class lolData
    {
        public List<playerStatSummaries> playerStatSummaries { get; set; }
        public int summonerId { get; set; }
    }

    public class playerStatSummaries
    {
        public int wins { get; set; }
        public aggregatedStats aggregatedStats { get; set; }
        public int losses { get; set; }
        public double modifyDate { get; set; }
        public string playerStatSummaryType { get; set; }
    }

    public class aggregatedStats
    {
        public int totalChampionKills { get; set; }
        public int totalTurretsKilled { get; set; }
        public int totalMinionKills { get; set; }
        public int totalNeutralMinionsKilled { get; set; }
        public int totalAssists { get; set; }
    }

    public class LoLUser
    {
        public int id { get; set; }
        public string name { get; set; }
        public int profileIconId { get; set; }
        public int revisionDate { get; set; }
        public int summonerLevel { get; set; }

        public EmbedBuilder GetEmbed() =>
           new EmbedBuilder()
            .WithColor(new Color(4, 97, 247))
            .WithAuthor(x => { x.Name = "League of Legends"; x.IconUrl =  ("https://yt3.ggpht.com/-AEerXPqHm3M/AAAAAAAAAAI/AAAAAAAAAAA/S8WpkwxItLQ/s900-c-k-no-mo-rj-c0xffffff/photo.jpg"); })
            .AddField(x => x.WithName("Name").WithValue($"{name}").WithIsInline(true))
            .AddField(x => x.WithName("Level").WithValue($"{summonerLevel}").WithIsInline(true))
            .AddField(x => x.WithName("ID").WithValue($"id{id}").WithIsInline(true));
    }
}

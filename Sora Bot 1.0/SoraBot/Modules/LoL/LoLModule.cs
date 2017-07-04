using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Services.LeagueOfLegends;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Modules.LoL
{
    public class LoLModule : ModuleBase<SocketCommandContext>
    {
        private lolService _lolService;

        public LoLModule(lolService ser)
        {
            _lolService = ser;
        }

        [Command("summoner", RunMode = RunMode.Async), Summary("Gets data about the summoner")]
        public async Task GetSummoner([Summary("Region")] string region, [Summary("User name"), Remainder]string name)
        {
            await _lolService.GetUser(Context, region, name);
        }

        [Command("summonerstats", RunMode = RunMode.Async), Alias("lolstats"), Summary("Gets stats about the summoner")]
        public async Task GetSummonerStats([Summary("Region")] string region, [Summary("User name"), Remainder]string name)
        {
            await _lolService.GetUserStats(Context, region, name);
        }
    }
}

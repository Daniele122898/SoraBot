using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Sora_Bot_1.SoraBot.Services;

namespace Sora_Bot_1.SoraBot.Modules.UbModule
{
    public class Ub : ModuleBase<SocketCommandContext>
    {
        private UbService _ubService;
        public Ub(UbService ser)
        {
            _ubService = ser;
        }

        [Command("urbandictionary", RunMode = RunMode.Async), Alias("ub", "ud", "urban")]
        [Summary("Pulls a Urban Dictionary Definition")]
        public async Task GetUbDef([Summary("Definition to search"),Remainder] string urban)
        {
            await _ubService.GetUbDef(Context, urban);
        }

    }
}

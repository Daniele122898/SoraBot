using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Services.YT;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Modules.YT
{
    public class YTModule : ModuleBase<SocketCommandContext>
    {

        private YTService _ytService;

        public YTModule(YTService ser)
        {
            _ytService = ser;
        }

        [Command("yt", RunMode = RunMode.Async), Summary("Makes a YT API search Query")]
        [RequireOwner]
        public async Task GetYTData([Summary("Query to send"),Remainder]string query)
        {
            await _ytService.GetYTResults(Context, query);
        }
    }
}

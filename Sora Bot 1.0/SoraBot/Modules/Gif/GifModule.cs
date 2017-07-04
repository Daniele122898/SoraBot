using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Services.Giphy;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Modules.Gif
{
    public class GifModule : ModuleBase<SocketCommandContext>
    {

        private GifService _gifService;
        public GifModule(GifService ser)
        {
            _gifService = ser;
        }

        [Command("gif", RunMode = RunMode.Async), Summary("Gives random Gif with specified search query")]
        public async Task GetRandomGif([Summary("name of gif to search"), Remainder]string query)
        {
            await _gifService.GetGifBySearch(Context, query);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Sora_Bot_1.SoraBot.Services;

namespace Sora_Bot_1.SoraBot.Modules.ImdbModule
{
    public class Imdb : ModuleBase
    {

        ImdbService _imdbService;
        public Imdb(ImdbService ser)
        {
            _imdbService = ser;
        }

        [Command("movie"), Alias("imdb"), Summary("Gets Movies/Series from IMDB")]
        public async Task GetImdb([Summary("Movie/Series to search"), Remainder] string target)
        {
            await _imdbService.GetImdb(Context, target);
        }
    }
}

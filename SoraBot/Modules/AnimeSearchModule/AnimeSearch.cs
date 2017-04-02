using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Sora_Bot_1.SoraBot.Services;

namespace Sora_Bot_1.SoraBot.Modules.AnimeSearchModule
{
    public class AnimeSearch : ModuleBase
    {
        private AnimeService _animeService;

        public AnimeSearch(AnimeService ser)
        {
            _animeService = ser;
        }

        [Command("anime", RunMode = RunMode.Async), Summary("Gets the stats of your desired Anime")]
        public async Task GetAnime([Summary("Anime to search"), Remainder]string anime)
        {
            await _animeService.GetAnime(Context, anime);
        }

        [Command("manga", RunMode = RunMode.Async), Summary("Gets the stats of your desired Manga")]
        public async Task GetManga([Summary("Manga to search"), Remainder]string manga)
        {
            await _animeService.GetManga(Context, manga);
        }

    }
}

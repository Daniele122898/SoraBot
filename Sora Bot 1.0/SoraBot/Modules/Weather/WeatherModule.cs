using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord;
using Sora_Bot_1.SoraBot.Services.Weather;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Modules.Weather
{
    public class WeatherModule : ModuleBase<SocketCommandContext>
    {
        private WeatherService _weatherService;

        public WeatherModule(WeatherService ser)
        {
            _weatherService = ser;
        }

        [Command("weather", RunMode = RunMode.Async), Summary("Gets the weather of the specified city")]
        public async Task GetWeather([Summary("City to get the weather for"), Remainder]string query)
        {
            await _weatherService.GetWeather(Context, query);
        }
    }
}

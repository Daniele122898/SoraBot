using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Services.Weather
{
    public class WeatherService
    {
        private string weatherID;
        public WeatherService()
        {
            var configDict = ConfigService.ConfigService.getConfig();
            if (!configDict.TryGetValue("weather", out weatherID))
            {
                Console.WriteLine("COULDN'T GET WEATHER API ID FROM CONFIG!");
            }
        }
        public async Task GetWeather(SocketCommandContext Context, string query)
        {
            try
            {
                var search = System.Net.WebUtility.UrlEncode(query);
                string response = "";
                using (var http = new HttpClient())
                {
                    response = await http.GetStringAsync($"http://api.openweathermap.org/data/2.5/weather?q={search}&appid={weatherID}&units=metric").ConfigureAwait(false);
                }
                var data = JsonConvert.DeserializeObject<WeatherData>(response);

                await Context.Channel.SendMessageAsync("", embed: data.GetEmbed());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find weather for city");
            }

        }
    }
}

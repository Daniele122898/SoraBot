using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Sora_Bot_1.SoraBot.Services
{
    public class PlayingWith
    {
        private DiscordSocketClient client;

        private string[] playing = new[]
        {
            //Users
            "with Karen",
            "with Emily",
            "with Nep",
            "with 0xFADED <3",
            "with Serraniel",
            "with Serenity",
            "with Shiro <3",

            //Games
            "CS:GO",
            "CoD 360 NS",

            //Languaes
            "with async",
            "with Discord.NET",

            //characters
            "with Rem <3",
            "with Emilia",
            "with little School Kids",
            "with Shiro <3",
            "with Inori"
        };

        public PlayingWith(DiscordSocketClient _c)
        {
            client = _c;
            Task.Factory.StartNew(() => { ChangePlayingStatus(); });
            
        }

        private async Task ChangePlayingStatus()
        {
            try
            {
                Random rand = new Random();
                while (true)
                {
                    if (client.ConnectionState.ToString() == "Connected")
                    {
                        await client.SetGameAsync(playing[rand.Next(playing.Length - 1)]);
                    }
                    await Task.Delay(10000);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Services.ConfigService;

namespace Sora_Bot_1.SoraBot.Core
{
    public class Program
    {
        public DiscordSocketClient client;
        private CommandHandler commands;
        private ConcurrentDictionary<string, string> configDict = new ConcurrentDictionary<string, string>();

        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        public async Task Run()
        {
            ConfigService.InitializeLoader();
            ConfigService.LoadConfig();
            configDict = ConfigService.getConfig();

            client = new DiscordSocketClient(new DiscordSocketConfig() {
                LogLevel = LogSeverity.Info,
                AudioMode = AudioMode.Outgoing,
                MessageCacheSize = 50
            });

            client.Log += (message) =>
            {
                if (!message.ToString().Contains("Unknown OpCode (Speaking)"))
                {
                    Console.WriteLine($"{message.ToString()}");
                }
                return Task.CompletedTask;
            };

            //Place the token of your bot account here
            string token; //= File.ReadAllText("token2.txt");
            if (!configDict.TryGetValue("token1", out token))
            {
                throw new Exception("FAILED TO GET TOKEN");
            }

            
            //configure the client to use a bot token and use our token
            await client.LoginAsync(TokenType.Bot, token);
            //connect the cline tot discord gateway
            await client.ConnectAsync();

            commands = new CommandHandler();
            await commands.Install(client);

            //Block this task until the program is exited
            await Task.Delay(-1);
        }


        public async void DisconnectAsync()
        {
            await client.DisconnectAsync();
            await client.LogoutAsync();
        }

    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sora_Bot_1.SoraBot.Modules.OwnerModule;
using Sora_Bot_1.SoraBot.Services;
using Sora_Bot_1.SoraBot.Services.ConfigService;
using Sora_Bot_1.SoraBot.Services.EPService;
using Sora_Bot_1.SoraBot.Services.Giphy;
using Sora_Bot_1.SoraBot.Services.GlobalSoraBans;
using Sora_Bot_1.SoraBot.Services.LeagueOfLegends;
using Sora_Bot_1.SoraBot.Services.Marry;
using Sora_Bot_1.SoraBot.Services.Mod;
using Sora_Bot_1.SoraBot.Services.PatService;
using Sora_Bot_1.SoraBot.Services.RateLimit;
using Sora_Bot_1.SoraBot.Services.Reminder;
using Sora_Bot_1.SoraBot.Services.StarBoradService;
using Sora_Bot_1.SoraBot.Services.TagService;
using Sora_Bot_1.SoraBot.Services.UserBlacklist;
using Sora_Bot_1.SoraBot.Services.Weather;
using Sora_Bot_1.SoraBot.Services.YT;

namespace Sora_Bot_1.SoraBot.Core
{
    public class Program
    {
        public DiscordSocketClient client;
        private ConcurrentDictionary<string, string> configDict = new ConcurrentDictionary<string, string>();
        private string token;


        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        public async Task Run()
        {
            ConfigService.InitializeLoader();
            ConfigService.LoadConfig();
            configDict = ConfigService.getConfig();

            client = createClient().Result;

            //client = new DiscordSocketClient();

            /*
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
            };*/

            //Place the token of your bot account here
            //= File.ReadAllText("token2.txt");
            if (!configDict.TryGetValue("token2", out token))
            {
                throw new Exception("FAILED TO GET TOKEN");
            }

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            //configure the client to use a bot token and use our token
            await client.LoginAsync(TokenType.Bot, token);
            //connect the cline tot discord gateway
            await client.StartAsync();

            //commands = new CommandHandler();
            //await commands.Install(client);
             
            //client.Disconnected += Client_Disconnected;

            //Block this task until the program is exited
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {

            return new ServiceCollection()
                // Base
                .AddSingleton(client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                .AddLogging()
                .AddSingleton<LogService>()
                // Extra
                //.AddSingleton(_config)
                // Add additional services here...
                .AddSingleton<MusicService>()
                .AddSingleton<UserGuildUpdateService>()
                .AddSingleton<RatelimitService>()
                .AddSingleton<StarBoardService>()
                .AddSingleton<AfkSertvice>()
                .AddSingleton<SelfRoleService>()
                .AddSingleton<UbService>()
                .AddSingleton<ImdbService>()
                .AddSingleton<ModService>()
                .AddSingleton<ReminderService>()
                .AddSingleton<WeatherService>()
                .AddSingleton<YTService>()
                .AddSingleton<BlackListService>()
                .AddSingleton<MusicService >()
                .AddSingleton<lolService>()
                .AddSingleton<RatelimitService2>()
                .AddSingleton<GifService>()
                .AddSingleton<MarryService>()
                .AddSingleton<AnimeService>()
                .AddSingleton<TagService>()
                .AddSingleton<PatService>()
                .AddSingleton<EPService>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<GlobalBanService>()
                .BuildServiceProvider();
        }

        private async Task<DiscordSocketClient> createClient()
        {
            var _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 75
            });

            _client.Log += (message) =>
            {
                if (!message.ToString().Contains("Unknown OpCode (Speaking)"))
                {
                    Console.WriteLine($"{message.ToString()}");
                }
                return Task.CompletedTask;
            };
            return _client;
        }

        public async void DisconnectAsync()
        {
            await client.LogoutAsync();
            await client.StopAsync();

        }

    }
}
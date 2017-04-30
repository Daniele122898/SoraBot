using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Modules.OwnerModule;
using Sora_Bot_1.SoraBot.Services;
using Sora_Bot_1.SoraBot.Services.ConfigService;

namespace Sora_Bot_1.SoraBot.Core
{
    public class Program
    {
        public DiscordSocketClient client;
        private CommandHandler commands;
        private ConcurrentDictionary<string, string> configDict = new ConcurrentDictionary<string, string>();
        private string token;

        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        public async Task Run()
        {
            ConfigService.InitializeLoader();
            ConfigService.LoadConfig();
            configDict = ConfigService.getConfig();

            client = createClient().Result;
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

            
            //configure the client to use a bot token and use our token
            await client.LoginAsync(TokenType.Bot, token);
            //connect the cline tot discord gateway
            await client.StartAsync();

            commands = new CommandHandler();
            await commands.Install(client);
             
            //client.Disconnected += Client_Disconnected;

            //Block this task until the program is exited
            await Task.Delay(-1);
        }

        private async Task Owner_reconnectToDisc()
        {
            try
            {
                Console.WriteLine("TRYING TO RECONNECT");
                await Task.Delay(20000);
                client = createClient().Result;
                await client.LoginAsync(TokenType.Bot, token);
                //await client.ConnectAsync(); TODO StartAsync();
                await client.StartAsync();
                commands = new CommandHandler();
                await commands.Install(client);
                client.Disconnected += Client_Disconnected;
                await SentryService.SendMessage($"**THE BOT HAS DISCONNECTED AND SUCCESFULLY RECONNECTED**");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async Task Client_Disconnected(Exception e)
        {
            try
            {
                await Task.Delay(20000);
                if (client.ConnectionState == ConnectionState.Connected)
                {
                    await SentryService.SendMessage("**Disconnected and Wrapper reconnected himself**");
                    return;
                }
                Console.WriteLine("TRYING TO RECOVER WITH RECONNECT");
                Console.WriteLine(e);
                //await SentryService.SendError(e);
                client = createClient().Result;
                await client.LoginAsync(TokenType.Bot, token);
                //await client.ConnectAsync(); TODO StartAsync();
                commands = new CommandHandler();
                await commands.Install(client);
                client.Disconnected += Client_Disconnected;
                await SentryService.SendMessage($"**THE BOT HAS DISCONNECTED AND SUCCESFULLY RECONNECTED**\n{e}");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            
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
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;

namespace Sora_Bot_1.SoraBot.Core
{
    public class Program
    {
        public DiscordSocketClient client;
        private CommandHandler commands;

        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        public async Task Run()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig() {
                LogLevel = LogSeverity.Info,
                AudioMode = AudioMode.Outgoing
            });

            client.Log += (message) => {
                Console.WriteLine($"{message.ToString()}");
                return Task.CompletedTask;
            };

            //Place the token of your bot account here
            string token = File.ReadAllText("token.txt");

            /*
        //Hook into the messagereceuved event on DiscordSocketclient
        client.MessageReceived += async (message) =>
        {
            //Check to see if the message content is $ping
            if (message.Content == "$ping")
                //send pong back to the channel the message was sent int
                await message.Channel.SendMessageAsync("pong :ping_pong:");
        };
        */

            
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
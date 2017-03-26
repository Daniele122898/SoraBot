using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Sora_Bot_1.SoraBot.Services
{
    public static class SentryService
    {
        public static DiscordSocketClient client;
        private static SocketUser serenity;


        public async static void Install()
        {
            try
            {
                var guild = client.GetGuild(180818466847064065);
                await guild.DownloadUsersAsync();
                serenity = guild.GetUser(192750776005689344);
                Console.WriteLine($"Got user {serenity.Username}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //await (await Context.User.CreateDMChannelAsync()).SendMessageAsync("",false,eb);

        public static async Task SendError(Exception e)
        {
            try
            {
                var err = e.ToString();
                if (err.Length <2000)
                {
                    var eb = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247),
                        Title = "Exception Occured",
                        Description = err
                    };
                    await (await serenity.CreateDMChannelAsync()).SendMessageAsync("", false, eb);
                }
                else
                {
                    string errE = err.Substring(2000);
                    string errS = err.Remove(2000);
                    var eb = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247),
                        Title = "Exception Occured 1",
                        Description = errS
                    };

                    await (await serenity.CreateDMChannelAsync()).SendMessageAsync("", false, eb);

                    var eb2 = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247),
                        Title = "Exception Occured 2",
                        Description = errE
                    };
                    await (await serenity.CreateDMChannelAsync()).SendMessageAsync("", false, eb2);
                }
                
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public static async Task SendMessage(string message)
        {
            try
            {
                if (serenity == null)
                    return;
                await (await serenity.CreateDMChannelAsync()).SendMessageAsync(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task SendError(Exception e, CommandContext Context)
        {
            try
            {
                var err = e.ToString();
                if(err.Length < 1700)
                {
                    var eb = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247),
                        Title = "Exception Occured",
                        Description = $"**INFO ABOUT COMMAND**\n"+
                                $"**Guild Name:**\t{Context.Guild.Name}\n" +
                                $"**Guild ID:**\t{Context.Guild.Id}\n" +
                                $"**User:**\t{Context.User.Username}#{Context.User.Discriminator}\n" +
                                $"**Message that Caused the Exception**\n" +
                                $"{Context.Message.Content}\n\n{err}"
                    };
                    await (await serenity.CreateDMChannelAsync()).SendMessageAsync("", false, eb);
                }
                else
                {
                    string errE = err.Substring(1700);
                    string errS = err.Remove(1700);
                    var eb = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247),
                        Title = "Exception Occured 1",
                        Description = $"**INFO ABOUT COMMAND**\n" +
                                $"**Guild Name:**\t{Context.Guild.Name}\n" +
                                $"**Guild ID:**\t{Context.Guild.Id}\n" +
                                $"**User:**\t{Context.User.Username}#{Context.User.Discriminator}\n" +
                                $"**Message that Caused the Exception**\n" +
                                $"{Context.Message.Content}\n\n{errS}"
                    };

                    await (await serenity.CreateDMChannelAsync()).SendMessageAsync("", false, eb);

                    var eb2 = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247),
                        Title = "Exception Occured 2",
                        Description = errE
                    };
                    await (await serenity.CreateDMChannelAsync()).SendMessageAsync("", false, eb2);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
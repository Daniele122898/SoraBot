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
        private static IUser serenity;


        public static void Install()
        {
            try
            {
                serenity = client.GetUser(192750776005689344);
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
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247)
                };

                eb.AddField((efb) =>
                {
                    efb.Name = "Exception Occured";
                    efb.IsInline = true;
                    efb.Value = e.ToString();
                });
                await (await serenity.CreateDMChannelAsync()).SendMessageAsync("", false, eb);
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
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247)
                };

                eb.AddField((efb) =>
                {
                    efb.Name = "Info About Command";
                    efb.IsInline = true;
                    efb.Value = $"**Guild Name:**\t{Context.Guild.Name}\n" +
                                $"**Guild ID:**\t{Context.Guild.Id}\n" +
                                $"**User:**\t{Context.User.Username}#{Context.User.Discriminator}\n" +
                                $"**Message that Caused the Exception**\n" +
                                $"{Context.Message.Content}";
                });
                eb.AddField((efb) =>
                {
                    efb.Name = "Exception Occured";
                    efb.IsInline = true;
                    efb.Value = e.ToString();
                });
                await (await serenity.CreateDMChannelAsync()).SendMessageAsync("", false, eb);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Sora_Bot_1.SoraBot.Core;
using Sora_Bot_1.SoraBot.Services;


namespace Sora_Bot_1.SoraBot.Modules.OwnerModule
{
    [Group("o")]
    [RequireOwner]
    public class OwnerModule : ModuleBase
    {
        //PREFIX

        private readonly CommandHandler handler;
        private UserGuildUpdateService updateService;

        public OwnerModule(CommandHandler _handler, UserGuildUpdateService service)
        {
            handler = _handler;
            updateService = service;
        }

        [Command("prefix"), Summary("Changes the prefix of the bot")]
        [RequireOwner]
        public async Task ChangePrefixOwner([Summary("Prefix to change to")] string prefix)
        {
            await ReplyAsync($"Prefix in this Guild was changed to `{prefix}`");
            handler.UpdateDict(Context.Guild.Id, prefix);
            handler.SaveDatabase();
        }

        //END PREFIX

        //ANNOUNCEMENTS

        [Command("here"), Summary("Sets the Channel in which the message was written as Channel to announce")]
        [RequireOwner]
        public async Task SetAnnounceChannel()
        {
            await updateService.SetChannel(Context);
        }

        [Command("remove"), Summary("Removes current Announcement channel to stop the bot from announcing")]
        [RequireOwner]
        public async Task RemoveAnnounceChannel()
        {
            await updateService.RemoveChannel(Context);
        }

        //END ANNOUNCEMENTS

        //CREATE EXCEPTION

        [Command("exc")]
        [RequireOwner]
        public async Task CreateException()
        {
            try
            {
                int i2 = 0;
                int i = 10 / i2;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        [Command("invite")]
        [RequireOwner]
        public async Task CreateInvite(ulong id)
        {
            try
            {
                var RequestedGuild = await Context.Client.GetGuildAsync(id);
                IInviteMetadata GuildDefault =
                    await (await RequestedGuild.GetChannelAsync(RequestedGuild.DefaultChannelId) as IGuildChannel)
                        .CreateInviteAsync();
                await Context.Channel.SendMessageAsync("Invite link: " + GuildDefault.Url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        [Command("leaveGuild")]
        [RequireOwner]
        public async Task LeaveGuild(ulong id)
        {
            try
            {
                var client = Context.Client as DiscordSocketClient;
                await client.GetGuild(id).LeaveAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        [Command("shutdown")]
        [RequireOwner]
        public async Task ShutDown()
        {
            await Context.Channel.SendMessageAsync("I wanted to leave anyway :information_desk_person:");
            await Context.Client.DisconnectAsync();
        }

        [Command("ban")]
        [RequireOwner]
        public async Task BanUser(IUser user)
        {
            try
            {
                await Context.Guild.AddBanAsync(user, 0, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
            
        }

        [Command("kick")]
        [RequireOwner]
        public async Task KickUser(IUser user)
        {
            try
            {
                await (user as IGuildUser).KickAsync(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }

        }

        [Command("guildlist")]
        [RequireOwner]
        public async Task GuildList()
        {
            try
            {
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247)
                };

                eb.AddField((x) =>
                {
                    x.Name = "Guild List";
                    x.IsInline = true;
                    x.Value = "";
                    var guilds = (Context.Client as DiscordSocketClient).Guilds;
                    foreach (var g in guilds)
                    {
                        x.Value += $"**Name:**\t{g.Name}\n**ID:**\t{g.Id}\n";
                    }
                });

                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }
    }
}
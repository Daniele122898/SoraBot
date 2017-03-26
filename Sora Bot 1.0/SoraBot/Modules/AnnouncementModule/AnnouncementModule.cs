using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Services;

namespace Sora_Bot_1.SoraBot.Modules.AnnouncementModule
{
    [Group("announcements"), Alias("a")]
    public class AnnouncementModule : ModuleBase
    {
        private UserGuildUpdateService updateService;

        public AnnouncementModule(UserGuildUpdateService service)
        {
            updateService = service;
        }

        [Command("welcome"), Summary("Sets the Welcome Channel with custom Welcome message")]
        public async Task SetWelcome([Summary("Message to set, takes current channel"), Remainder]string message = null)
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to set the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.SetWelcome(Context, message);
        }

        [Command("welcomemsg"), Summary("Sets the custom Welcome message")]
        public async Task SetWelcomeMessage ([Summary("Message to set"), Remainder]string message = null)
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to set the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.SetWelcomeMessage(Context, message);
        }

        [Command("welcomechannel"), Alias("welcomecha"), Summary("Sets the Welcome channel")]
        public async Task SetWelcomeChannel([Summary("Channel to set")]IMessageChannel channel)
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to set the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.SetWelcomeChannel(Context, channel);
        }

        [Command("rmwelcome"), Summary("Removes the Welcome Channel and Welcome message")]
        public async Task RemoveWelcome()
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to remove the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.RemoveWelcome(Context);
        }

        [Command("leave"), Summary("Sets the Leave Channel with custom Leave message")]
        public async Task SetLeave([Summary("Message to set, takes current channel"), Remainder]string message = null)
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to set the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.SetLeave(Context, message);
        }

        [Command("leavemsg"), Summary("Sets the custom Leave message")]
        public async Task SetLeaveMessage([Summary("Message to set"), Remainder]string message = null)
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to set the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.SetLeaveMessage(Context, message);
        }

        [Command("leavechannel"), Alias("leavecha"), Summary("Sets the Leave channel")]
        public async Task SetLeaveChannel([Summary("Channel to set")]IMessageChannel channel)
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to set the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.SetLeaveChannel(Context, channel);
        }

        [Command("rmleave"), Summary("Removes the Welcome Channel and Welcome message")]
        public async Task RemoveLeave()
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to remove the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.RemoveLeave(Context);
        }
    }
}
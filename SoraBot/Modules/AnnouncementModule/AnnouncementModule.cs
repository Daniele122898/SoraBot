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
    public class AnnouncementModule : ModuleBase
    {
        private UserGuildUpdateService updateService;

        public AnnouncementModule(UserGuildUpdateService service)
        {
            updateService = service;
        }

        [Command("here"), Summary("Sets the Channel in which the message was written as Channel to announce")]
        public async Task SetAnnounceChannel()
        {
            if (!((SocketGuildUser) Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to set the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.SetChannel(Context);
        }

        [Command("remove"), Summary("Removes current Announcement channel to stop the bot from announcing")]
        public async Task RemoveAnnounceChannel()
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to remove the announcement channel! You need Manage Channels permissions!");
                return;
            }
            await updateService.RemoveChannel(Context);
        }
    }
}
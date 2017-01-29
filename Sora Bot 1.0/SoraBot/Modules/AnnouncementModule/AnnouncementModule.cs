using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetAnnounceChannel()
        {
            await updateService.SetChannel(Context);
        }

        [Command("remove"), Summary("Removes current Announcement channel to stop the bot from announcing")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveAnnounceChannel()
        {
            await updateService.RemoveChannel(Context);
        }
    }
}
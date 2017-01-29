using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.VisualBasic;
using Sora_Bot_1.SoraBot.Core;
using Sora_Bot_1.SoraBot.Services;


namespace Sora_Bot_1.SoraBot.Modules.OwnerModule
{
    [Group("o")]
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
    }
}

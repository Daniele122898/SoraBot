using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Services.StarBoradService;

namespace Sora_Bot_1.SoraBot.Modules.StarBoardModule
{
    public class StarBoardModule : ModuleBase<SocketCommandContext>
    {
        private StarBoardService starBoardService;

        public StarBoardModule(StarBoardService service)
        {
            starBoardService = service;
        }

        [Command("star"), Summary("Sets the Channel in which Star reactions should be posted")]
        public async Task SetStarBoardChannel()
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to set the starboard channel! You need Manage Channels permissions!");
                return;
            }
            await starBoardService.SetChannel(Context);
        }

        [Command("starremove"), Summary("Removes current Starboard channel!")]
        public async Task RemoveStarBoardChannel()
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to remove the starboard channel! You need Manage Channels permissions!");
                return;
            }
            await starBoardService.RemoveChannel(Context);
        }
    }
}

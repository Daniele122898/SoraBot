using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Services.StarBoradService;

namespace Sora_Bot_1.SoraBot.Modules.StarBoardModule
{
    public class StarBoardModule : ModuleBase
    {
        private StarBoardService starBoardService;

        public StarBoardModule(StarBoardService service)
        {
            starBoardService = service;
        }

        [Command("star"), Summary("Sets the Channel in which Star reactions should be posted")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task SetStarBoardChannel()
        {
            await starBoardService.SetChannel(Context);
        }

        [Command("starremove"), Summary("Removes current Starboard channel!")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task RemoveStarBoardChannel()
        {
            await starBoardService.RemoveChannel(Context);
        }
    }
}

using Discord.Commands;
using Sora_Bot_1.SoraBot.Services;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Modules.AFKModule
{
    public class Afk : ModuleBase<SocketCommandContext>
    {
        private AfkSertvice _afkService;
        public Afk(AfkSertvice afkService)
        {
            _afkService = afkService;
        }

        [Command("afk"), Alias("away"), Summary("Sets you AFK with a specified message to deliver to anyone that mentions you")]
        public async Task ToggleAFK([Summary("Message to deliver when you get mentioned"), Remainder]string msg = "")
        {
            await _afkService.ToggleAFK(Context, msg);
        }
    }
}

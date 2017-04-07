using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Threading.Tasks;
using Sora_Bot_1.SoraBot.Services.Mod;

namespace Sora_Bot_1.SoraBot.Modules.Mod
{
    public class ModModule : ModuleBase
    {

        private ModService _modService;

        public ModModule(ModService ser)
        {
            _modService = ser;
        }

        [Command("ban"), Alias("permban"), Summary("Permabans a user and deletes his messages in the past 48 hours")]
        public async Task PermBanUser([Summary("User to ban")] IUser user, [Summary("Reason for ban"), Remainder]string reason = null)
        {
            await _modService.PermBan(Context, user, reason);
        }

        [Command("punishlogs"), Alias("punish"), Summary("Sets Channel for punishlogs")]
        public async Task SetPunishLogs([Summary("Channel for logs, if left blank it will take the current one")] IMessageChannel channelT = null)
        {
            var channel = channelT ?? Context.Channel;
            await _modService.setPunishLogsChannel(Context, channel);
        }

        [Command("rmpunishlogs"), Alias("rmpunish"), Summary("Removes the Punishlogs Channel")]
        public async Task RemovePunishLogs()
        {
            await _modService.delPunishLogsChannel(Context);
        }

        [Command("reason"), Summary("Adds a reason to a Case")]
        public async Task AddReason([Summary("Reason to update"), Remainder]string reason)
        {
            await _modService.AddReason(Context, reason);
        }

        [Command("kick"), Summary("Kicks the user")]
        public async Task KickUser([Summary("User to kick")] IUser user, [Summary("Reason for kick"), Remainder]string reason = null)
        {
            await _modService.Kick(Context, user, reason);
        }

    }
}

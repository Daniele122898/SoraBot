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

        [Command("ban", RunMode = RunMode.Async), Alias("permban"), Summary("Permabans a user and deletes his messages in the past 48 hours")]
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

        [Command("reason", RunMode = RunMode.Async), Summary("Adds a reason to a Case")]
        public async Task AddReason([Summary("Reason to update"), Remainder]string reason)
        {
            await _modService.AddReason(Context, reason);
        }

        [Command("kick", RunMode = RunMode.Async), Summary("Kicks the user")]
        public async Task KickUser([Summary("User to kick")] IUser user, [Summary("Reason for kick"), Remainder]string reason = null)
        {
            await _modService.Kick(Context, user, reason);
        }

        [Command("warn", RunMode = RunMode.Async), Summary("Warns the user")]
        public async Task WarnUser([Summary("User to warn")] IUser user, [Summary("Reason for warn"), Remainder]string reason = null)
        {
            await _modService.WarnUser(Context, user, reason);
        }

        [Command("rmwarn", RunMode = RunMode.Async), Summary("Removes warnings from the user")]
        public async Task RmWarnUser([Summary("User to warn")] IUser user, [Summary("Reason for warn")]int amount = 999)
        {
            await _modService.RemoveWarnings(Context, user, amount);
        }

        [Command("cases", RunMode = RunMode.Async), Alias("listcase", "listcases"), Summary("Lists all cases of specified user")]
        public async Task ListCaseUsers([Summary("User to list cases")] IUser user)
        {
            await _modService.ListCases(Context, user);
        }

        //MODLOG
        [Command("modlog"), Alias("log"), Summary("Sets Channel for ModLogs")]
        public async Task SetModLogs([Summary("Channel for logs, if left blank it will take the current one")] IMessageChannel channelT = null)
        {
            var channel = channelT ?? Context.Channel;
            await _modService.setModLgosChannel(Context, channel);
        }

        [Command("rmmodlog"), Alias("rmlog"), Summary("Removes the ModLog Channel")]
        public async Task RemoveModLogs()
        {
            await _modService.removeModLogsChannel(Context);
        }

        [Command("modconfig"), Alias("config"), Summary("Shows the ModLog Config")]
        public async Task ModLogConfig()
        {
            await _modService.ShowConfigLog(Context);
        }

        [Command("modconfig role"), Alias("modconfig rolechange"), Summary("Toggles the ModLog Role Change")]
        public async Task ModLogsRoleToggle()
        {
            await _modService.ToggleRole(Context);
        }

        [Command("modconfig channel"), Alias("modconfig channelchange"), Summary("Toggles the ModLog Channel Change")]
        public async Task ModLogsChannelToggle()
        {
            await _modService.ToggleChannel(Context);
        }

        [Command("modconfig server"), Alias("modconfig server"), Summary("Toggles the ModLog Server Change")]
        public async Task ModLogsServerToggle()
        {
            await _modService.ToggleServer(Context);
        }

        [Command("modconfig msgdelete"), Alias("modconfig msg"), Summary("Toggles the ModLog Msg Delete")]
        public async Task ModLogsMsgToggle()
        {
            await _modService.ToggleMessage(Context);
        }

    }
}

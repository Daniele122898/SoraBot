using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Sora_Bot_1.SoraBot.Core;

namespace Sora_Bot_1.SoraBot.Modules.DynamicPrefixModule
{
    public class DynamicPrefixModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandHandlingService handler;

        public DynamicPrefixModule(CommandHandlingService _handler)
        {
            handler = _handler;
        }

        [Command("prefix"), Summary("Changes the prefix of the bot")]
        public async Task ChangePrefix([Summary("Prefix to change to")] string prefix)
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.Administrator))
            {
                await ReplyAsync(":no_entry_sign: You don't have permission to set the perfix! You need Administrator permissions!");
                return;
            }
            await ReplyAsync($"Prefix in this Guild was changed to `{prefix}`");
            handler.UpdateDict(Context.Guild.Id, prefix);
            handler.SaveDatabase();
        }

        [Command("prefix"), Summary("Checks the current prefix for the guild")]
        public async Task CheckPrefix()
        {
            await ReplyAsync($"Prefix for this Guild is `{handler.GetPrefix(Context.Guild.Id)}`");
        }
    }
}
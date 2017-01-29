using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.VisualBasic;
using Sora_Bot_1.SoraBot.Core;

namespace Sora_Bot_1.SoraBot.Modules.DynamicPrefixModule
{
    public class DynamicPrefixModule : ModuleBase
    {
        private readonly CommandHandler handler;

        public DynamicPrefixModule(CommandHandler _handler)
        {
            handler = _handler;
        }

        [Command("prefix"), Summary("Changes the prefix of the bot")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ChangePrefix([Summary("Prefix to change to")] string prefix)
        {
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
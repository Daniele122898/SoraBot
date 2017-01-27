using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Core;

namespace Sora_Bot_1.SoraBot.Modules.DynamicPrefixModule
{
    public class DynamicPrefixModule : ModuleBase
    {

        private CommandHandler handler;

        public DynamicPrefixModule(CommandHandler _handler)
        {
            handler = _handler;
        }

        [Command("prefix"), Summary("Changes the prefix of the bot")]
        public async Task ChangePrefix([Summary("Prefix to change to")] string prefix)
        {
            await ReplyAsync($"Prefix in this Guild was changed to `{prefix}`");
            handler.UpdateDict(Context.Guild.Id, prefix);
            handler.SaveDatabase();
        }

    }
}

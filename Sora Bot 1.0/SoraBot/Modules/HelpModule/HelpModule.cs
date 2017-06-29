using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Core;

namespace Sora_Bot_1.SoraBot.Modules.HelpModule
{
    [Group("help")]
    [Alias("h")]
    public class HelpModule : ModuleBase<SocketCommandContext> //<SocketCommandContext>
    {

        private CommandService service;

        public HelpModule(CommandService serviceA) // Create a constructor for the commandservice dependency
        {
            service = serviceA;
        }

        [Command(""), Summary("List of all available commands.")]
        public async Task Help([Summary("Command of which the help should be displayed"), Remainder]string cmdName = null)
        {
            bool pm= true;
            DiscordSocketClient _client = Context.Client as DiscordSocketClient;
            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247)
            };
            if(cmdName != null)
            {
                pm = false;
                eb.Title = $"Help for *{cmdName}*";
                var found = false;
                foreach (var c in service.Commands)
                {
                    if(!c.Aliases.Contains(cmdName))
                        continue;

                    eb.AddField((efb) =>
                    {
                        efb.Name = c.Parameters.Aggregate(c.Name + "\n",
                            (current, cmd) => $"{current} {(cmd.IsOptional ? $"[<{cmd.Name}>]" : $"<{cmd.Name}>")}");
                        efb.Value =
                            c.Parameters.Aggregate(
                                c.Summary + "\n\n" +
                                c.Aliases.Aggregate("**Aliases**\n",
                                    (current, alias) =>
                                        $"{current}{(c.Aliases.ElementAt(0) == alias ? string.Empty : ", ")}{alias}") +
                                "\n\n**Parameters** ",
                                (current, cmd) =>
                                    $"{current}\n{cmd.Name} {(cmd.IsOptional ? "(optional)" : "")}: {cmd.Summary}") +
                            "\n\n**Permissions**\n";
                        if (c.Preconditions.Count > 0)
                        {
                            efb.Value += "Manage Channels";
                        }
                    });
                    found = true;
                }
                if(!found)
                    throw new ArgumentException($"Command \"{cmdName}\" not found");
            }
            if (!pm)
            {
                await Context.Channel.SendMessageAsync("", false, eb);
            }
            else
            {
                //await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync("",false,eb);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} help can be found here <http://git.argus.moe/serenity/SoraBot/wikis/sora-help>");
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Services.ChangelogService;

namespace Sora_Bot_1.SoraBot.Modules.ChangelogModule
{
    public class ChangelogModule : ModuleBase
    {
        [Command("changelog"), Summary("Prints the Changelog")]
        [Alias("updates","change")]
        public async Task GetChangelog()
        {
            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247),
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.AvatarUrl
                }
            };

            eb.AddField((x) =>
            {
                x.Name = "Changelog";
                x.IsInline = true;
                x.Value = ChangelogService.changelog;
            });

            await Context.Channel.SendMessageAsync("", false, eb);
        }
    }
}

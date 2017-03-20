using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Services.TagService;

namespace Sora_Bot_1.SoraBot.Modules.TagModule
{
    [Group("tag")]
    [Alias("t")]
    public class TagModule : ModuleBase
    {
        private TagService tagService;

        public TagModule(TagService _tagService)
        {
            tagService = _tagService;
        }

        [Command("create"), Summary("Creates a Tag"), Priority(1)]
        [Alias("add")]
        public async Task CreateTag([Summary("tag to create <tag | what to display>"), Remainder] string tagEntry)
        {
            await tagService.CreateTag(tagEntry, Context);
        }

        [Command("remove"), Summary("Removes specified Tag"), Priority(1)]
        public async Task RemoveTag([Summary("tag to remove"), Remainder] string tag)
        {
            await tagService.RemoveTag(tag, Context);
        }

        [Command("taglist"), Summary("Lists all tags"), Priority(1)]
        public async Task ListTags()
        {
            await tagService.ListTags(Context);
        }

        [Command(""), Summary("Displays the value of the Tag specified if it exists"), Priority(0)]
        public async Task SearchTag([Summary("tag to search"), Remainder] string tag)
        {
            await tagService.SearchTagAndSend(tag, Context);
        }

        [Command("restrict"),
         Summary(
             "Restricts the Tag Command to the specified permissions (If no permission is entered the restriction will be removed!) => `ManageChannels , Administrator, KickMembers, BanMembers, ManageGuild`"
         ), Priority(1)]
        public async Task RestrictTask(
            [Summary("Choose one of the permissions to add, if none given the current restriction will be removed")] string perm = null)
        {
            await tagService.RestrictManageChannels(Context, (perm == null ? perm : perm.ToLower()));
        }
    }
}
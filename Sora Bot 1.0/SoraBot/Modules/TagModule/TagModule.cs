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

        [Command("create"), Summary("Creates a Tag")]
        [Alias("add")]
        public async Task CreateTag([Summary("tag to create <tag | what to display>"), Remainder] string tagEntry)
        {
            await tagService.CreateTag(tagEntry, Context);
        }

        [Command(""), Summary("Searches for your tag if existing")]
        public async Task SearchTag([Summary("tag to search"), Remainder] string tag)
        {
            await tagService.SearchTagAndSend(tag, Context);
        }

        [Command("taglist"), Summary("Lists all tags")]
        public async Task ListTags()
        {
            await tagService.ListTags(Context);
        }
    }
}
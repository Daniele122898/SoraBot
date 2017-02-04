using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sora_Bot_1.SoraBot.Services.TagService
{
    public class TagService
    {
        private ConcurrentDictionary<ulong, List<TagStruct>> tagDict = new ConcurrentDictionary<ulong, List<TagStruct>>();
        private JsonSerializer jSerializer = new JsonSerializer();

        public TagService()
        {
            InitializeLoader();
            LoadDatabase();
        }

        public async Task CreateTag(string entry, CommandContext Context)
        {
            try
            {
                string[] tag = entry.Split('|');
                if (!String.IsNullOrEmpty(tag[0]) && !String.IsNullOrEmpty(tag[1]) && tag.Length == 2)
                {
                    TagStruct tagStruct = new TagStruct
                    {
                        tag = tag[0].Trim(),
                        value = tag[1].Trim()
                    };
                    List<TagStruct> tagList = new List<TagStruct>();
                    if (tagDict.ContainsKey(Context.Guild.Id))
                    {
                        tagDict.TryGetValue(Context.Guild.Id, out tagList);

                        foreach (var t in tagList)
                        {
                            if (t.tag.Equals(tagStruct.tag))
                            {
                                await Context.Channel.SendMessageAsync(":no_entry_sign: Tag already exists!");
                                return;
                            }
                        }

                        tagList.Add(tagStruct);
                        tagDict.TryUpdate(Context.Guild.Id, tagList);
                    }
                    else
                    {
                        tagList.Add(tagStruct);
                        tagDict.TryAdd(Context.Guild.Id, tagList);
                    }
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
                        x.Name = "Successfully Added Tag!";
                        x.IsInline = true;
                        x.Value = $"**Tag**\n" +
                                  $"{tag[0]}\n" +
                                  $"**Value**\n" +
                                  $"{tag[1]}";
                    });
                    SaveDatabase();
                    await Context.Channel.SendMessageAsync("",false,eb);
                }
                else
                {
                    await Context.Channel.SendMessageAsync(
                        $":no_entry_sign: Failed to add tag! Make sure its <tag | what to do when tag is called>");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task ListTags(CommandContext Context)
        {
            if (tagDict.ContainsKey(Context.Guild.Id))
            {

                List<TagStruct> tagStruct = new List<TagStruct>();
                tagDict.TryGetValue(Context.Guild.Id, out tagStruct);

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
                    x.Name = "Tags in this Guild";
                    x.Value = "";
                    foreach (var t in tagStruct)
                    {
                        x.Value += $"**{t.tag}**\n";
                    }
                });
                await Context.Channel.SendMessageAsync("", false, eb);
            }
            else
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: No tags in this Guild yet!");
            }
        }

        public async Task SearchTagAndSend(string tag, CommandContext Context)
        {
            List<TagStruct> tagStruct = new List<TagStruct>();
            if (tagDict.ContainsKey(Context.Guild.Id))
            {
                tagDict.TryGetValue(Context.Guild.Id, out tagStruct);
                foreach (var t in tagStruct)
                {
                    if (t.tag.Equals(tag))
                    {
                        await Context.Channel.SendMessageAsync(t.value);
                        return;
                    }
                }
                await Context.Channel.SendMessageAsync($":no_entry_sign: Tag `{tag}` was not found!");
            }
            else
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: There are no tags in this guild!");
            }
        }

        private void InitializeLoader()
        {
            jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public void SaveDatabase()
        {
            using (StreamWriter sw = File.CreateText(@"Tags.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    jSerializer.Serialize(writer, tagDict);
                }
            }
        }

        private void LoadDatabase()
        {
            if (File.Exists("Tags.json"))
            {
                using (StreamReader sr = File.OpenText(@"Tags.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        tagDict = jSerializer.Deserialize<ConcurrentDictionary<ulong, List<TagStruct>>>(reader);
                    }
                }
            }
            else
            {
                File.Create("Tags.json").Dispose();
            }
        }

        public struct TagStruct
        {
            public string tag;
            public string value;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sora_Bot_1.SoraBot.Services;
using Sora_Bot_1.SoraBot.Services.ChangelogService;
using Sora_Bot_1.SoraBot.Services.EPService;
using Sora_Bot_1.SoraBot.Services.PatService;
using Sora_Bot_1.SoraBot.Services.StarBoradService;
using Sora_Bot_1.SoraBot.Services.TagService;

namespace Sora_Bot_1.SoraBot.Core
{
    public class CommandHandler
    {
        private DiscordSocketClient client;
        private CommandService commands;
        private DependencyMap map;
        private CommandHandler handler => this;
        private MusicService musicService;
        private UserGuildUpdateService updateService;
        private StarBoardService starBoardService;
        private ReminderService remService;
        private AfkSertvice _afkService;
        private PatService patService;
        private ImdbService _imdbService;
        private SelfRoleService _selfRoleService;
        private AnimeService _animeService;
        private TagService tagService;
        private UbService _ubService;
        private RatelimitService ratelimitService;
        private EPService epService;
        private PlayingWith playingWith;
        public static Dictionary<ulong, string> prefixDict = new Dictionary<ulong, string>();
        private JsonSerializer jSerializer = new JsonSerializer();

        public async Task Install(DiscordSocketClient c)
        {
            InitializeLoader();
            LoadDatabase();
            client = c;
            updateService = new UserGuildUpdateService();
            musicService = new MusicService();
            ratelimitService = new RatelimitService();
            starBoardService = new StarBoardService(client, ratelimitService);
            _afkService = new AfkSertvice();
            _selfRoleService = new SelfRoleService();
            _ubService = new UbService();
            _imdbService = new ImdbService();
            _animeService = new AnimeService();
            //remService = new ReminderService();

            tagService = new TagService();
            patService = new PatService();
            epService = new EPService(client);
            playingWith = new PlayingWith(client);
            SentryService.client = client;
            //SentryService.Install();

            commands = new CommandService();
            map = new DependencyMap();


            map.Add(musicService);
            map.Add(handler);
            map.Add(_afkService);
            map.Add(_selfRoleService);
            //map.Add(commands);
            map.Add(updateService);
            map.Add(_imdbService);
            map.Add(_animeService);
            map.Add(patService);
            map.Add(_ubService);
            map.Add(tagService);
            map.Add(starBoardService);
            map.Add(epService);
            //map.Add(remService);

            //Discover all of the commands in this assembly and load them
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            //Hook the messagereceive event into our command handler

            ChangelogService.LoadChangelog();
            client.MessageReceived += epService.IncreaseEP;
            client.MessageReceived += _afkService.Client_MessageReceived;
            client.MessageReceived += HandleCommand;
            client.UserJoined += updateService.UserJoined;
            client.UserLeft += updateService.UserLeft;
            client.ReactionAdded += starBoardService.StarAddedNew;
            client.ReactionRemoved += starBoardService.StarRemovedNew;
            client.UserVoiceStateUpdated += musicService.CheckIfAlone;
            client.JoinedGuild += Client_JoinedGuild;
            client.LeftGuild += Client_LeftGuild;
            client.GuildAvailable += Client_GuildAvailable;
        }

        

        private async Task Client_GuildAvailable(SocketGuild guild)
        {
            if(guild.Id == 180818466847064065)
            {
                SentryService.Install();
            }
        }

        private async Task Client_LeftGuild(SocketGuild arg)
        {
            try
            {
                await SentryService.SendMessage($"Left Guild {arg.Name} / {arg.Id} with {arg.MemberCount} members");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        private async Task Client_JoinedGuild(SocketGuild guild)
        {
            try
            {
                bool send = true;
                foreach (var p in guild.DefaultChannel.PermissionOverwrites)
                {
                    if (p.Permissions.SendMessages == PermValue.Deny)
                        send = false;
                }
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Bot created by Serenity#0783"
                    },
                    Title = $"Hello {guild.Name} :wave:",
                    Description =
                            $"Thank you for inviting me. My name is Sora and I hope I can make your Guild a little bit better :)" +
                            $" Sora's functions are extensively documented so that you can use him to his full potential.\nFor the docs [click this link](http://git.argus.moe/serenity/SoraBot/wikis/sora-help)"
                };
                eb.AddField((x) =>
                {
                    x.Name = "Live Support and Feedback";
                    x.IsInline = false;
                    x.Value =
                        "If you need live support or simply want to give me feedback / bug reports or issue a Feature request head to this guild:\n[Click to Join](https://discord.gg/Pah4yj5)";
                });
                if (!send)
                {
                    SocketTextChannel channel = null;
                    bool pes = true;
                    foreach (var cha in guild.TextChannels)
                    {
                        pes = true;
                        foreach (var per in cha.PermissionOverwrites)
                        {
                            if (per.Permissions.SendMessages == PermValue.Deny)
                            {
                                pes = false;
                                break;
                            }
                        }
                        if (pes == true)
                        {
                            channel = cha;
                            break;
                        }
                    }
                    eb.AddField((x) =>
                    {
                        x.Name = "Sorry :(";
                        x.IsInline = false;
                        x.Value = "I could not send a message in the default channel of this guild and had to take another one. That is why this message may be in a random channel. Sorry!";
                    });
                    await channel.SendMessageAsync("", false, eb);
                }
                else
                {
                    await guild.DefaultChannel.SendMessageAsync("", false, eb);
                }
                await SentryService.SendMessage($"Joined Guild {guild.Name} / {guild.Id} with {guild.MemberCount} members");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        private void InitializeLoader()
        {
            jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public void SaveDatabase()
        {
            using (StreamWriter sw = File.CreateText(@"guildPrefix.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    jSerializer.Serialize(writer, prefixDict);
                }
            }
        }

        public void UpdateDict(ulong ID, string prefix)
        {
            if (prefixDict.ContainsKey(ID))
            {
                prefixDict[ID] = prefix;
            }
            else
            {
                prefixDict.Add(ID, prefix);
            }
        }

        public string GetPrefix(ulong ID)
        {
            if (prefixDict.ContainsKey(ID))
            {
                return prefixDict[ID];
            }
            else
            {
                return "$";
            }
        }

        private void LoadDatabase()
        {
            if (File.Exists("guildPrefix.json"))
            {
                using (StreamReader sr = File.OpenText(@"guildPrefix.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        prefixDict = jSerializer.Deserialize<Dictionary<ulong, string>>(reader);
                    }
                }
            }
            else
            {
                File.Create("guildPrefix.json").Dispose();
            }
        }


        public async Task HandleCommand(SocketMessage messageParam)
        {
            //Don't process the comand if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;


            //Create a command Context
            var context = new CommandContext(client, message);
            if (context.IsPrivate)
                return;
            if (context.User.IsBot)
                return;

            string prefix;
            if (!prefixDict.TryGetValue(context.Guild.Id, out prefix))
            {
                prefix = "$";
            }
            //create a number to track where the prefix ends and the command begins
            int argPos = prefix.Length-1;
            //Determine if the message is a command based on if it starts with ! or a mention prefix
            if (
                !(message.HasStringPrefix(prefix, ref argPos) ||
                  message.HasMentionPrefix(client.CurrentUser, ref argPos)))
                return;

            //Send to Ratelimiter Service
            if(await ratelimitService.onlyCheck(context.User, context.Guild, context))
                return;

            //Execute the command. (result does no indicate a return value
            // rather an object starting if the command executed successfully
            /*
            IResult result;
            try
            {
                result = await commands.ExecuteAsync(context, argPos, map);
                if (result.IsSuccess)
                    await ratelimitService.checkRatelimit(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, context);
            }*/
            var result = await commands.ExecuteAsync(context, argPos, map);

            if (result.IsSuccess)
                await ratelimitService.checkRatelimit(context.User);

            //if (!result.IsSuccess)
            //  await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
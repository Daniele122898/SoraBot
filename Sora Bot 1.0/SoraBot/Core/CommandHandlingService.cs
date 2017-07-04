using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sora_Bot_1.SoraBot.Services;
using Sora_Bot_1.SoraBot.Services.ChangelogService;
using Sora_Bot_1.SoraBot.Services.EPService;
using Sora_Bot_1.SoraBot.Services.GlobalSoraBans;
using Sora_Bot_1.SoraBot.Services.Mod;
using Sora_Bot_1.SoraBot.Services.RateLimit;
using Sora_Bot_1.SoraBot.Services.StarBoradService;
using Sora_Bot_1.SoraBot.Services.UserBlacklist;

namespace Sora_Bot_1.SoraBot.Core
{
    public class CommandHandlingService
    {

        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private IServiceProvider _provider;
        private PlayingWith _playingWith;

        private int _commandsRan = 0;
        public static Dictionary<ulong, string> prefixDict = new Dictionary<ulong, string>();
        private JsonSerializer jSerializer = new JsonSerializer();

        private EPService _epService;
        private AfkSertvice _afkSertvice;
        private UserGuildUpdateService _updateService;
        private StarBoardService _starBoardService;
        private MusicService _musicService;
        private ModService _modService;
        private GlobalBanService _globalBans;
        private readonly BlackListService _blackListService;
        private readonly RatelimitService2 _ratelimitService2;
        private PlayingWith _playingService;


        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
        {
            InitializeLoader();
            LoadDatabase();
            _discord = discord;
            _commands = commands;
            _provider = provider;

            _epService = _provider.GetService<EPService>();
            _afkSertvice = _provider.GetService<AfkSertvice>();
            _updateService = _provider.GetService<UserGuildUpdateService>();
            _starBoardService = _provider.GetService<StarBoardService>();
            _musicService = _provider.GetService<MusicService>();
            _modService = _provider.GetService<ModService>();
            _blackListService = _provider.GetService<BlackListService>();
            _ratelimitService2 = _provider.GetService<RatelimitService2>();
            _globalBans = _provider.GetService<GlobalBanService>();
            _playingService = new PlayingWith(_discord);

            SentryService.client = _discord;


            ChangelogService.LoadChangelog();

            _discord.MessageReceived += _epService.IncreaseEP;
            _discord.MessageReceived += _afkSertvice.Client_MessageReceived;
            _discord.UserJoined += _updateService.UserJoined;
            _discord.UserLeft += _updateService.UserLeft;
            _discord.ReactionAdded += _starBoardService.StarAddedNew;
            _discord.ReactionRemoved += _starBoardService.StarRemovedNew;
            _discord.UserVoiceStateUpdated += _musicService.CheckIfAlone;

            //Bans

            _discord.UserBanned += _modService.Client_UserBanned;
            _discord.UserUnbanned += _modService.Client_UserUnbanned;

            //Modlog

            _discord.MessageDeleted += _modService.Client_MessageDeleted;
            _discord.RoleCreated += _modService.Client_RoleCreated;
            _discord.RoleDeleted += _modService.Client_RoleDeleted;
            _discord.RoleUpdated += _modService.Client_RoleUpdated;

            _discord.ChannelCreated += _modService.Client_ChannelCreated;
            _discord.ChannelDestroyed += _modService.Client_ChannelDestroyed;
            _discord.ChannelUpdated += _modService.Client_ChannelUpdated;

            _discord.GuildUpdated += _modService.Client_GuildUpdated;

            //Owner

            _discord.MessageReceived += MessageReceived;
            _discord.GuildAvailable += Client_GuildAvailable;
            _discord.JoinedGuild += Client_JoinedGuild;
            _discord.LeftGuild += Client_LeftGuild;
        }


        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            // Add additional initialization code here...
        }

        private async Task Client_GuildAvailable(SocketGuild guild)
        {
            if (guild.Id == 180818466847064065)
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

        public int CommandsRunSinceRestart()
        {
            return _commandsRan;
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var context = new SocketCommandContext(_discord, message);

            if (context.IsPrivate)
                return;

            if (_globalBans.IsBanned(context.User.Id))
                return;

            if (_blackListService.CheckIfBlacklisted(context))
                return;


            string prefix;
            if (!prefixDict.TryGetValue(context.Guild.Id, out prefix))
            {
                prefix = "$";
            }

            int argPos = prefix.Length - 1;
            if (
                !(message.HasStringPrefix(prefix, ref argPos) ||
                  message.HasMentionPrefix(_discord.CurrentUser, ref argPos)))
                return;

            if (_ratelimitService2.CheckIfRatelimited(context.User))
                return;


            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            if (result.IsSuccess)
            {
                //await ratelimitService.checkRatelimit(context.User);
                await _ratelimitService2.RateLimitMain(context.User);
                _commandsRan++;
            }
        }

    }
}

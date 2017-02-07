using System;
using System.Collections.Generic;
using System.IO;
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
        private PatService patService;
        private TagService tagService;
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
            starBoardService = new StarBoardService(client);
            musicService = new MusicService();
            //remService = new ReminderService();

            tagService = new TagService();
            patService = new PatService();
            epService = new EPService(client);
            playingWith = new PlayingWith(client);
            SentryService.client = client;
            SentryService.Install();

            commands = new CommandService();
            map = new DependencyMap();


            map.Add(musicService);
            map.Add(handler);
            map.Add(commands);
            map.Add(updateService);
            map.Add(patService);
            map.Add(tagService);
            map.Add(starBoardService);
            map.Add(epService);
            //map.Add(remService);

            //Discover all of the commands in this assembly and load them
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            //Hook the messagereceive event into our command handler

            ChangelogService.LoadChangelog();
            client.MessageReceived += epService.IncreaseEP;
            client.MessageReceived += HandleCommand;
            client.UserJoined += updateService.UserJoined;
            client.UserLeft += updateService.UserLeft;
            client.ReactionAdded += starBoardService.StarAdded;
            client.ReactionRemoved += starBoardService.StarRemoved;
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

            //Execute the command. (result does no indicate a return value
            // rather an object starting if the command executed successfully
            var result = await commands.ExecuteAsync(context, argPos, map);
            //if (!result.IsSuccess)
            //  await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
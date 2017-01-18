using System;
using System.Linq;
using SoraBot.Src.Sora.Modules.TestModule;
using SoraBot.Src.Sora.Modules.Google;
using SoraBot.Src.Sora.Modules.Music;

using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.Audio;

namespace SoraBot
{
    class Program
    {
        static void Main(string[] args) => new Program().Start(); //new non-static instance of programm and run Start.

        private DiscordClient _client;

        public void Start()
        {
            _client = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Debug;
                x.LogHandler = Log;
            })
            .UsingCommands(x =>
            {
                x.PrefixChar = '$';
                x.HelpMode = HelpMode.Public;
            })
            .UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
            })
            .UsingModules();

            _client.AddModule<TestModule>("Test", ModuleFilter.None);
            _client.AddModule<Google>("Google", ModuleFilter.None);
            _client.AddModule<Music>("Music", ModuleFilter.None);

                        
            /*
            _client.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor)
                {
                    await e.Channel.SendMessage(e.Message.Text);
                }
            };*/

            _client.UserJoined += async (s, e) =>
            { //announcements
                var logChannel = e.Server.FindChannels("announcements").FirstOrDefault();
                await logChannel.SendMessage($"The User {e.User.Name} joined the Guild!");
            };

            _client.UserLeft += async (s, e) =>
            { //announcements
                var logChannel = e.Server.FindChannels("announcements").FirstOrDefault();
                await logChannel.SendMessage($"The User {e.User.Name} left the Guild!");
            };

            _client.GetService<CommandService>().CreateCommand("greet")
                .Alias("gr", "hi")
                .Description("Greets a person")
                .Parameter("GreetedPerson", ParameterType.Required)
                .Do(async e => {
                    await e.Channel.SendMessage($"{e.User.Name} greets {e.GetArg("GreetedPerson")}");
                });

            _client.GetService<CommandService>().CreateGroup("do", cgb => {

                cgb.CreateCommand("greet")
                .Alias("gr", "hi")
                .Description("Greets a person")
                .Parameter("GreetedPerson", ParameterType.Required)
                .Do(async e => {
                    await e.Channel.SendMessage($"{e.User.Name} greets {e.GetArg("GreetedPerson")}");
                });

                cgb.CreateCommand("bye")
                .Alias("bb", "gb")
                .Description("Greets a person.")
                .Parameter("GreetedPerson", ParameterType.Required)
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"{e.User.Name} says goodbye to {e.GetArg("GreetedPerson")}");
                });

            });

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect("", TokenType.Bot);
            });
        }//END START

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"[{e.Severity}] [{e.Source}] {e.Message}");
        }
    }
}

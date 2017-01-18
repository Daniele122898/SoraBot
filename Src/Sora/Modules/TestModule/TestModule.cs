using Discord;
using Discord.Modules;

namespace SoraBot.Src.Sora.Modules.TestModule
{
    internal class TestModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("", group => {

                group.CreateCommand("test")
                .Alias("tes")
                .Do(async e => {
                    await e.Channel.SendMessage("Test Module");
                });

            });

        }

    }
}

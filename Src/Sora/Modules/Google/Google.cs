using Discord;
using Discord.Commands;
using Discord.Modules;

namespace SoraBot.Src.Sora.Modules.Google
{
    internal class Google : IModule
    {

        private ModuleManager _manager;
        private DiscordClient _client;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("", group =>
            {

                group.CreateCommand("google")
                .Description("Googles the parameter for you")
                .Parameter("Search", ParameterType.Unparsed)
                .Do(async e =>
                {
                    string search = e.GetArg("Search").Replace(" ", "%20");
                    await e.Channel.SendMessage("<https://lmgtfy.com/?q="+search+">");
                });

            });
        }

    }
}

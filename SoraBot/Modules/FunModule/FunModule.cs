using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Sora_Bot_1.SoraBot.Modules.FunModule
{
    public class FunModule : ModuleBase
    {
        //$say hello -> hello
        [Command("say"), Summary("Echos a message")]
        public async Task Say([Remainder, Summary("The text to echo")] string echo)
        {
            //ReplyAsync is a mtheod on modulebase
            await ReplyAsync(echo);
        }

        [Command("git"), Summary("Links to my GitLab page")]
        [Alias("github", "gitlab")]
        public async Task Git()
        {
            await ReplyAsync("http://git.argus.moe/serenity/SoraBot");
        }

        [Command("feedback"), Summary("How to give feedback or suggestions")]
        [Alias("suggestions", "suggestion", "bug", "bugreport")]
        public async Task Feedback()
        {
            await ReplyAsync("To give me feedback, suggestions or tell me about any bugs please join\n https://discord.gg/Pah4yj5");
        }

        [Command("door"), Summary("Shows the specified user the door. Mostly used after bad jokes")]
        [Alias("out")]
        public async Task Door([Summary("User to show the door")] IUser user)
        {
            await ReplyAsync($"{user.Username} :point_right::skin-tone-1: :door:");
        }

        //$lenny lenny -> ( ͡° ͜ʖ ͡°)
        [Command("lenny"), Summary("Posts a lenny face")]
        public async Task Lenny()
        {
            await ReplyAsync("( ͡° ͜ʖ ͡°)");
        }

        [Command("google"), Summary("Will google for you")]
        public async Task Google([Summary("Subject to google"), Remainder] string google)
        {
            string search = google.Replace(" ", "%20");
            await Context.Channel.SendMessageAsync("<https://lmgtfy.com/?q=" + search + ">");
        }

        //$swag  ( ͡° ͜ʖ ͡°)>⌐■-■ -> ( ͡⌐■ ͜ʖ ͡-■)
        [Command("swag"), Summary("Swags the chat")]
        public async Task Swag()
        {
            var msg = await ReplyAsync("( ͡° ͜ʖ ͡°)>⌐■-■");
            await Task.Delay(1500);
            await msg.ModifyAsync(x => { x.Content = "( ͡⌐■ ͜ʖ ͡-■)"; });
        }

        [Command("about"), Summary("Gives an about page")]
        public async Task AboutInfo()
        {
            var eb = new EmbedBuilder()
            {
                Title = "About",
                Color = new Color(4, 97, 247),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.GetAvatarUrl()
                }
            };
            eb.AddField((efb) =>
            {
                efb.Name = "Creator";
                efb.IsInline = true;
                efb.Value =
                    "This bot has been created by Serenity#0783.\nIf you want to directly chat with me join this Guild:\n[Click to join](https://discord.gg/Pah4yj5)";
            });
            eb.AddField((efb) =>
            {
                efb.Name = "Properties";
                efb.IsInline = true;
                efb.Value =
                    "I was written in C# using the Discord.NET 1.0 API.\nFor more info about that use the `info` command\nor look at my source code at [Gitlab](http://git.argus.moe/serenity/SoraBot)";
            });
            eb.AddField((efb) =>
            {
                efb.Name = "About me";
                efb.IsInline = true;
                efb.Value = "Who am I you may ask?\n" +
                            "My name is Sora and I'm a member of Imanity.\n" +
                            "My birthday is on the 3th of June and I'm currently 18 years old.\n" +
                            "I have a little but lovely Stepsister called Shiro. She and I together form the infamous duo『　』also known as blank.\n" +
                            "I am here to rule over this new found world and I will overthrow Tet as the current God. That is my ultimate goal.\n" +
                            "If you feel confident enough you can always try to challenge me and I will make sure to leave you dazzled";
            });
            await ReplyAsync("", false, eb);
        }

        [Command("ping"), Summary("Gives the ping of the Server on which the bot resides and the discord servers")]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! {(Context.Client as DiscordSocketClient).Latency} ms :ping_pong:");
        }

        [Command("invite"), Summary("Gives an invite link to invite Sora to your own Guild!")]
        [Alias("inv")]
        public async Task InviteAsync()
        {
            var eb = new EmbedBuilder()
            {
                Title = "Invite Sora",
                Color = new Color(4, 97, 247),
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Description =
                    "Just uncheck the permissions you dont feel like giving, this might break Sora though. At least give him these permissions:\n" +
                    "Read/Send Messages, Embed Links, Attach Files, Mention Everyone, Add Reactions, Read Message History\n" +
                    "Connect to voice and speak!\n" +
                    "[Click to Invite](https://discordapp.com/oauth2/authorize?client_id=270931284489011202&scope=bot&permissions=2146958463)"
            };

            await ReplyAsync("", false, eb);
        }
    }
}
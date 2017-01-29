using System.Threading.Tasks;
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

        //$lenny lenny -> ( ͡° ͜ʖ ͡°)
        [Command("lenny"), Summary("Posts a lenny face")]
        public async Task Lenny()
        {
            await ReplyAsync("( ͡° ͜ʖ ͡°)");
        }

        [Command("google"), Summary("Will google for you")]
        public async Task Google([Summary("Subject to google"), Remainder]string google)
        {
            string search = google.Replace(" ", "%20");
            await Context.Channel.SendMessageAsync("<https://lmgtfy.com/?q=" + search + ">");
        }

        //$swag  ( ͡° ͜ʖ ͡°)>⌐■-■ -> ( ͡⌐■ ͜ʖ ͡-■)
        [Command("swag"), Summary("Swags the chat")]
        public async Task Swag()
        {
            var msg = await ReplyAsync("( ͡° ͜ʖ ͡°)>⌐■-■");
            await Task.Delay(1000);
            await msg.ModifyAsync(x =>
            {
                x.Content = "( ͡⌐■ ͜ʖ ͡-■)";
            });
        }

        [Command("about"), Summary("Gives an about page")]
        public async Task AboutInfo()
        {
            await ReplyAsync("This bot was made by Serenity using Discord.Net");
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
            await ReplyAsync("To invite Me just open this link and choose the Server:\nhttps://discordapp.com/oauth2/authorize?client_id=270931284489011202&scope=bot&permissions=30720");
        }

    }

}

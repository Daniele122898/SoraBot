using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Services;
using Discord.Addons.InteractiveCommands;

namespace Sora_Bot_1.SoraBot.Modules.AudioModule
{
    //[Group("music")]
    //[Alias("m")]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private MusicService musicService;
        private InteractiveService _interactive;

        public AudioModule(MusicService _musicService, InteractiveService inter)
        {
            musicService = _musicService;
            _interactive = inter;
        }

        [Command("join", RunMode = RunMode.Async), Summary("Joines the channel of the User")]
        public async Task JoinChannel()
        {
            //Get the audio channel
            var channel = (Context.Message.Author as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync(
                    "User must be in a voice channel, or a voice channel must be passed as an argument");
                return;
            }

            await musicService.JoinChannel(channel, Context.Guild.Id);
        }

        [Command("add", RunMode = RunMode.Async), Summary("Adds selected song to Queue")]
        public async Task AddToQueue([Summary("URL to add"), Remainder] string url)
        {
            //Check if url?
            if (url.Contains("http://") || url.Contains("https://"))
            {
                await musicService.AddQueue(url, Context);
            }
            else
            {
                await musicService.AddQueueYT(Context, url, _interactive);
            }
        }

        [Command("addyt", RunMode = RunMode.Async), Summary("Adds selected song to Queue")]
        public async Task AddToQueueYT([Summary("Name of yt video to add"), Remainder] string name)
        {
            await musicService.AddQueueYT(Context, name, _interactive);
        }

        [Command("skip", RunMode = RunMode.Async), Summary("Skip current song in queue")]
        public async Task SkipQueue()
        {
            await musicService.SkipQueueEntry(Context);
        }

        [Command("clear", RunMode = RunMode.Async),
         Summary("Clears the entire music queue, requires Manage Channels permission though")]
        public async Task ClearQ()
        {
            if (((SocketGuildUser) Context.User).GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await musicService.ClearQueue(Context);
            }
            else
            {
                await ReplyAsync(":no_entry_sign: You don't have the Manage Channels permission to clear the Queue!");
            }
            
        }

        [Command("list"), Summary("Shows a list of all songs in the Queue")]
        [Alias("queue")]
        public async Task List()
        {
            await musicService.QueueList(Context);
        }

        [Command("np"), Summary("Tells you which song is currently playing")]
        public async Task NowPlaying()
        {
            await musicService.NowPlaying(Context);
        }

        [Command("play", RunMode = RunMode.Async), Summary("Plays the qurrent queue")]
        public async Task PlayQueue()
        {
            await musicService.PlayQueue(Context);
        }

        [Command("leave", RunMode = RunMode.Async), Summary("Leaves the voice channel in which the User is in.")]
        public async Task LeaveChannel()
        {
            var channel = (Context.Message.Author as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync(
                    "User must be in a voice channel, or a voice channel must be passed as an argument");
                return;
            }

            await musicService.LeaveChannel(Context, channel);
        }

        /*
        [Command("play", RunMode = RunMode.Async), Summary("Plays a YT URL")]
        public async Task PlayMusic([Summary("URL to play")] string url)
        {
            await musicService.PlayMusic(url, Context);
        }*/

        [Command("stop", RunMode = RunMode.Async), Summary("Stops the current Audioplayer")]
        public async Task StopMusic()
        {
            await musicService.StopMusic(Context);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Services;

namespace Sora_Bot_1.SoraBot.Modules.AudioModule
{
    //[Group("music")]
    //[Alias("m")]
    public class AudioModule : ModuleBase
    {
        private MusicService musicService;

        public AudioModule(MusicService _musicService)
        {
            musicService = _musicService;
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
        public async Task AddToQueue([Summary("URL to add")] string url)
        {
            await musicService.AddQueue(url, Context);
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
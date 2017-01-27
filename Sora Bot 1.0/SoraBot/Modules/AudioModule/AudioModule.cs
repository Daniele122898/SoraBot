﻿using System;
using System.Collections.Generic;
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
    [Group("music")]
    [Alias("m")]
    public class AudioModule : ModuleBase
    {
        

        private MusicService musicService;

        public AudioModule(MusicService _musicService)
        {
            musicService = _musicService;
        }

        [Command("join", RunMode = RunMode.Async),Summary("Joines the channel of the User or the one passed as an argument")]
        public async Task JoinChannel([Summary("Channel to join")] IVoiceChannel channel = null)
        {
            //Get the audio channel
            channel = channel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync(
                    "User must be in a voice channel, or a voice channel must be passed as an argument");
                return;
            }

            await musicService.JoinChannel(channel, Context.Guild.Id);
        }

        [Command("add")]
        public async Task AddToQueue(string url)
        {
            await musicService.AddQueue(url, Context);
        }

        [Command("queue")]
        public async Task PlayQueue()
        {
            await musicService.PlayQueue(Context);
        }

        [Command("count")]
        public async Task CountQueue()
        {
            await musicService.CountQueue(Context);
        }
        
        [Command("leave"), Summary("Leaves the voice channel in which the user is in or the one passed as an argument")]
        public async Task LeaveChannel([Summary("Channel to leave")]IVoiceChannel channel = null)
        {
            channel = channel?? (Context.Message.Author as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync(
                    "User must be in a voice channel, or a voice channel must be passed as an argument");
                return;
            }

            await musicService.LeaveChannel(Context);
        }

        [Command("play", RunMode = RunMode.Async), Summary("Plays a YT URL")]
        public async Task PlayMusic([Summary("URL to play")] string url)
        {
            await musicService.PlayMusic(url, Context);
        }

        [Command("stop"), Summary("Stops the current Audioplayer")]
        public async Task StopMusic(IVoiceChannel channel = null)
        {
            await musicService.StopMusic(Context);
        }

        

    }
}
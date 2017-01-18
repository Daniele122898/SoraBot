using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.Audio;

namespace SoraBot.Src.Sora.Modules.Music
{
    public class Music : IModule
    {

        private ModuleManager _manager;
        private DiscordClient _client;

        public void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            IAudioClient _vClient = null;            

            manager.CreateCommands("m", group =>
            {

                group.CreateCommand("join")
                .Description("Joins the current Voice Channel of the calling user")
                .Parameter("VoiceChannelName", ParameterType.Unparsed)
                .Do(async e => {
                    var voiceChannel = e.User.VoiceChannel;
                    _vClient = await _client.GetService<AudioService>()
                    .Join(voiceChannel);
                });

                group.CreateCommand("leave")
                .Description("Leaves Voice Chat")
                .Do(async e =>
                    {
                        if (e.User.VoiceChannel == _vClient.Channel)
                        {
                            await _vClient.Disconnect();
                        }
                    });

                group.CreateCommand("play")
                .Description("Will play a song from URL")
                .Parameter("URL", ParameterType.Required)
                .Do(async e =>
                    {
                            await Task.Run(() =>
                            {
                                string[] url = e.GetArg("URL").Split('=');
                                if (!File.Exists(url[1] + ".wav"))
                                {
                                    System.Diagnostics.Process processB = new System.Diagnostics.Process();
                                    System.Diagnostics.ProcessStartInfo startInfo =
                                        new System.Diagnostics.ProcessStartInfo();
                                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                    startInfo.FileName = "cmd.exe";
                                    startInfo.Arguments =
                                        $"/C python youtube-dl -i -x --max-filesize 1024m --audio-format wav --audio-quality 0 --id {e.GetArg("URL")}";
                                    //{e.GetArg("URL")}   -o {e.GetArg("URL")}.%(ext)s
                                    //startInfo.Arguments = $"/C python youtube-dl --abprt-on-error --no-color --no-playlist --max-filesize 1024m -f bestaudio/best[height<=720][fps<=30]/best[height<=720]/[abr<=192] -x --audio-format wav --audio-quality 0 --id https://www.youtube.com/watch?v=o_Ay_iDRAbc";
                                    processB.StartInfo = startInfo;
                                    processB.Start();


                                    if (url[1] != null)
                                    {
                                        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                                        stopwatch.Start();
                                        while (!File.Exists(url[1] + ".wav") && stopwatch.ElapsedMilliseconds < 5000)
                                        {
                                        }
                                    }
                                    else
                                    {
                                        Task.Delay(3000);
                                    }
                                }
                                var process = Process.Start(new ProcessStartInfo
                                {
                                    FileName = "ffmpeg",
                                    Arguments = $"-i {url[1]}.wav " +
                                                "-f s16le -ar 48000 -ac 2 pipe:1",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true
                                });

                                Task.Delay(2000);

                                var blockSize = 3840;//3840
                                byte[] buffer = new byte[blockSize];
                                int byteCount;


                                e.Channel.SendMessage("Downloading and playing shortly");

                                while (true)
                                {
                                    byteCount = process.StandardOutput.BaseStream
                                        .Read(buffer, 0, blockSize);

                                    if (byteCount == 0)
                                        break;

                                    if (_vClient == null)
                                        break;
                                    _vClient.Send(buffer, 0, byteCount);

                                }
                                if (_vClient != null)
                                {
                                    _vClient.Wait();
                                }
                            });//TASK


                    });

            });
        }
    }
}

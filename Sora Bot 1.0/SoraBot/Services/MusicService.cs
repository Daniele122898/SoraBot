using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Audio;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Sora_Bot_1.SoraBot.Services
{
    public class MusicService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> audioDict =
            new ConcurrentDictionary<ulong, IAudioClient>();

        private readonly ConcurrentDictionary<IAudioClient, audioStream_Token> audioStreamDict =
            new ConcurrentDictionary<IAudioClient, audioStream_Token>();

        private ConcurrentDictionary<ulong, List<string>> queueDict = new ConcurrentDictionary<ulong, List<string>>();

        private List<string> songDataBase = new List<string>();
        private JsonSerializer jSerializer = new JsonSerializer();


        public async Task JoinChannel(IVoiceChannel channel, ulong guildID)
        {
            //for the next step with transmitting audio, you would want to pass this audio client in to a service
            var audioClient = await channel.ConnectAsync();
            audioDict.TryAdd(guildID, audioClient);
        }

        public async Task LeaveChannel(CommandContext Context)
        {
            IAudioClient aClient;
            audioDict.TryGetValue(Context.Guild.Id, out aClient);
            if (aClient == null)
            {
                await Context.Channel.SendMessageAsync("Bot is not connected to any Voice Channels");
                return;
            }
            await aClient.DisconnectAsync();
            audioDict.TryRemove(Context.Guild.Id, out aClient);
        }

        public async Task AddQueue(string url, CommandContext Context)
        {
            var msg =
                await Context.Channel.SendMessageAsync(":arrows_counterclockwise: Downloading and Adding to Queue...");
            string name = await Download(url, msg, Context);
            if (!name.Equals("f"))
            {
                List<string> tempList = new List<string>();
                if (queueDict.ContainsKey(Context.Guild.Id))
                {
                    queueDict.TryGetValue(Context.Guild.Id, out tempList);
                    tempList.Add(name);
                    queueDict.TryUpdate(Context.Guild.Id, tempList);
                }
                else
                {
                    tempList.Add(name);
                    queueDict.TryAdd(Context.Guild.Id, tempList);
                }
                //await Context.Channel.SendMessageAsync(":musical_note: Successfully Downloaded. Will play shortly");
            }
            else
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: failed");
            }
        }

        public async Task PlayQueue(CommandContext Context)
        {
            IAudioClient client;
            if (!audioDict.TryGetValue(Context.Guild.Id, out client))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: Bot must first join a Voice Channel!");
            }
            else
            {
                PlayQueueAsync(client, Context);
            }
        }

        public async Task SkipQueueEntry(CommandContext Context)
        {
            IAudioClient client;
            if (!audioDict.TryGetValue(Context.Guild.Id, out client))
            {
                List<string> queue = new List<string>();
                if (!queueDict.TryGetValue(Context.Guild.Id, out queue))
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: You first have to create a Queue by adding atleast one song!");
                }
                else
                {
                    queue.RemoveAt(0);
                    queueDict.TryUpdate(Context.Guild.Id, queue);
                    await Context.Channel.SendMessageAsync(":track_next: Skipped first entry in Queue");
                }
            }
            else
            {
                List<string> queue = new List<string>();
                if (!queueDict.TryGetValue(Context.Guild.Id, out queue))
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: You first have to create a Queue by adding atleast one song!");
                }
                else
                {
                    queue.RemoveAt(0);
                    queueDict.TryUpdate(Context.Guild.Id, queue);
                    if (queue.Count == 0)
                    {
                        await Context.Channel.SendMessageAsync(":track_next: Queue is now empty!");
                    }
                    else
                    {
                        PlayQueueAsync(client, Context);
                        await Context.Channel.SendMessageAsync(":track_next: Skipped first entry in Queue and started playing next song!");
                    }
                }
            }
        }

        private async Task PlayQueueAsync(IAudioClient client, CommandContext Context)
        {
            try
            {
                if (queueDict.ContainsKey(Context.Guild.Id))
                {
                    List<string> queue = new List<string>();
                    queueDict.TryGetValue(Context.Guild.Id, out queue);
                    for (int i = 1; i <= queue.Count;)
                    {
                        string name = queue[0];
                        var ffmpeg = CreateStream(name);
                        audioStream_Token strToken;
                        if (audioStreamDict.ContainsKey(client))
                        {
                            audioStreamDict.TryGetValue(client, out strToken);
                            strToken.tokenSource.Cancel();
                            strToken.tokenSource.Dispose();
                            strToken.tokenSource = new CancellationTokenSource();
                            strToken.token = strToken.tokenSource.Token;
                            audioStreamDict.TryUpdate(client, strToken);
                        }
                        else
                        {
                            strToken.audioStream = client.CreatePCMStream(960);
                            strToken.tokenSource = new CancellationTokenSource();
                            strToken.token = strToken.tokenSource.Token;
                            audioStreamDict.TryAdd(client, strToken);
                        }

                        var output = ffmpeg.StandardOutput.BaseStream; //1920, 2880, 960
                        await output.CopyToAsync(strToken.audioStream, 960, strToken.token).ContinueWith(task =>
                        {
                            if (!task.IsCanceled && task.IsFaulted) //supress cancel exception
                                Console.WriteLine(task.Exception);
                        });
                        ffmpeg.WaitForExit();
                        await strToken.audioStream.FlushAsync();
                        queueDict.TryGetValue(Context.Guild.Id, out queue);
                        queue.Remove(name);
                        queueDict.TryUpdate(Context.Guild.Id, queue);
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Please add a song to the Queue first!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /*
        public async Task PlayMusic(string url, CommandContext Context)
        {
            IAudioClient aClient;
            audioDict.TryGetValue(Context.Guild.Id, out aClient);
            string msg = ":arrows_counterclockwise: Downloading...";
            if (url.Equals("rand") || url.Equals("random"))
            {
                InitializeLoader();
                LoadDatabse();
            }
            var msgToEdit = await Context.Channel.SendMessageAsync(msg);
            //Task music = new Task(() => SendAsync(aClient, url, msgToEdit));
            //music.Start();
            await SendAsync(aClient, url, msgToEdit, Context);
        }*/

        public async Task StopMusic(CommandContext Context)
        {
            IAudioClient client;
            audioDict.TryGetValue(Context.Guild.Id, out client);

            audioStream_Token strAT;
            audioStreamDict.TryGetValue(client, out strAT);

            strAT.tokenSource.Cancel();
            strAT.tokenSource.Dispose();
            strAT.tokenSource = new CancellationTokenSource();
            strAT.token = strAT.tokenSource.Token;
            audioStreamDict.TryUpdate(client, strAT);
        }

        public async Task QueueList(CommandContext Context)
        {
            List<string> queue = new List<string>();
            if (queueDict.TryGetValue(Context.Guild.Id, out queue))
            {
                if (queue.Count != 0)
                {
                    try
                    {
                        var eb = new EmbedBuilder()
                        {
                            Color = new Color(4, 97, 247)
                        };

                        eb.Title = "Queue List";

                        var infoJsonT = File.ReadAllText($"{queue[0]}.info.json");
                        var infoT = JObject.Parse(infoJsonT);

                        var titleT = infoT["fulltitle"].ToString();

                        eb.AddField((efb) =>
                        {
                            efb.Name = "Now playing";
                            efb.IsInline = true;
                            efb.Value = titleT;
                        });
                        eb.AddField((efb) =>
                        {
                        efb.Name = "Queue";
                        efb.IsInline = true;
                            for (int i = 1; i < queue.Count; i++)
                            {
                                var infoJson = File.ReadAllText($"{queue[i]}.info.json");
                                var info = JObject.Parse(infoJson);

                                var title = info["fulltitle"].ToString();
                            
                                efb.Value += $"{i}. {title} \n";
                            
                            }
                        });

                        await Context.Channel.SendMessageAsync("", false, eb);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Queue is empty!");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(
                    ":no_entry_sign: You first have to create a Queue by adding atleast one song!");
            }
        }

        public async Task NowPlaying(CommandContext Context)
        {
            List<string> queue = new List<string>();
            if (queueDict.TryGetValue(Context.Guild.Id, out queue))
            {
                if (queue.Count != 0)
                {
                    var infoJson = File.ReadAllText($"{queue[0]}.info.json");
                    var info = JObject.Parse(infoJson);

                    var title = info["fulltitle"];

                    var eb = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247)
                    };

                    eb.AddField((efb) =>
                    {
                        efb.Name = "Now playing";
                        efb.IsInline = true;
                        efb.Value = title.ToString();
                    });

                    await Context.Channel.SendMessageAsync("", false, eb);
                }
                else
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Queue is empty!");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(
                    ":no_entry_sign: You first have to create a Queue by adding atleast one song!");
            }
        }

        private Process CreateStream(string path)
        {
            //-loglevel quiet
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {path}.mp3 -ac 2 -loglevel quiet -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            return Process.Start(ffmpeg);
        }

        private Process YtDl(string path)
        {
            var ytdl = new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = $"-i -x --no-playlist --max-filesize 100m --audio-format mp3 --audio-quality 0 --id {path} --write-info-json"
            };
            return Process.Start(ytdl);
        }


        private async Task<string> Download(string path, IUserMessage msg, CommandContext Context)
        {
            bool stream = false;
            string[] id = new string[2];
            string[] idL = path.Split('=');
            if (!path.Equals("random") && !path.Equals("rand") && idL[1] != null)
            {
                if (idL[1].Contains("&"))
                {
                    string[] temp = idL[1].Split('&');
                    idL[1] = temp[0];
                }
                id[1] = idL[1];
            }

            if (path.Equals("random") || path.Equals("rand"))
            {
                if (songDataBase.Count < 1)
                {
                    await msg.ModifyAsync(
                        x => { x.Content = ":no_entry_sign: Current Song Database is empty. Cannot play random song"; });
                }
                else
                {
                    Random rand = new Random();
                    int index = rand.Next(songDataBase.Count);
                    id[1] = "" + songDataBase[index];
                }
            }

            // Create FFmpeg using the previous example
            if (!File.Exists(id[1] + ".mp3"))
            {
                var ytdl = YtDl(path);
            }

            if (id[1] != null)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Start();
                while (!File.Exists(id[1] + ".mp3") && stopwatch.ElapsedMilliseconds < 30000)
                {
                }
                if (File.Exists(id[1] + ".mp3"))
                {
                    await msg.ModifyAsync(
                        x => { x.Content = ":musical_note: Successfully Downloaded."; });
                    if (!songDataBase.Contains(id[1]))
                    {
                        if (songDataBase.Count < 1)
                        {
                            LoadDatabse();
                        }
                        songDataBase.Add(id[1]);
                        SaveDatabase();
                    }
                    stream = true;
                }
                else
                {
                    await msg.ModifyAsync(x =>
                    {
                        x.Content =
                            ":no_entry_sign: Failed to Download. Possible reasons: Video is blocked in Bot's country, Video was too long, NO PLAYLISTS!";
                    });
                }
            }
            else
            {
                if (!path.Equals("rand") && !path.Equals("random"))
                {
                    await msg.ModifyAsync(x => { x.Content = "It must be a YT link! Failed to Download."; });
                }
            }
            if (stream)
            {
                return id[1];
            }
            else
            {
                return "f";
            }
        }

        /*
        private async Task SendAsync(IAudioClient client, string path, IUserMessage msg, CommandContext Context)
        {
            try
            {
                /*
                bool stream = false;
                LoadDatabse();
                string[] id = new string[2];
                string[] idL = path.Split('=');
                if (!path.Equals("random") && !path.Equals("rand") && idL[1] != null)
                {
                    if (idL[1].Contains("&"))
                    {
                        string[] temp = idL[1].Split('&');
                        idL[1] = temp[0];
                    }
                    id[1] = idL[1];
                }

                if (path.Equals("random") || path.Equals("rand"))
                {
                    if (songDataBase.Count < 1)
                    {
                        await msg.ModifyAsync(
                            x =>
                            {
                                x.Content = ":no_entry_sign: Current Song Database is empty. Cannot play random song";
                            });
                    }
                    else
                    {
                        Random rand = new Random();
                        int index = rand.Next(songDataBase.Count);
                        id[1] = "" + songDataBase[index];
                    }
                }

                // Create FFmpeg using the previous example
                if (!File.Exists(id[1] + ".mp3"))
                {
                    var ytdl = YtDl(path);
                }

                if (id[1] != null)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();
                    while (!File.Exists(id[1] + ".mp3") && stopwatch.ElapsedMilliseconds < 30000)
                    {
                    }
                    if (File.Exists(id[1] + ".mp3"))
                    {
                        await msg.ModifyAsync(
                            x => { x.Content = ":musical_note: Successfully Downloaded. Will play shortly"; });
                        if (!songDataBase.Contains(id[1]))
                        {
                            if (songDataBase.Count < 1)
                            {
                                LoadDatabse();
                            }
                            songDataBase.Add(id[1]);
                            SaveDatabase();
                        }
                        stream = true;
                    }
                    else
                    {
                        await msg.ModifyAsync(x =>
                        {
                            x.Content =
                                ":no_entry_sign: Failed to Download. Possible reasons: Video is blocked in Bot's country, Video was too long, NO PLAYLISTS!";
                        });
                    }
                }
                else
                {
                    if (!path.Equals("rand") && !path.Equals("random"))
                    {
                        await msg.ModifyAsync(x => { x.Content = "It must be a YT link! Failed to Download."; });
                    }
                }
                string name = await Download(path, msg, Context);

                if (!name.Equals("f"))
                {
                    var ffmpeg = CreateStream(name);
                    audioStream_Token strToken;
                    if (audioStreamDict.ContainsKey(client))
                    {
                        audioStreamDict.TryGetValue(client, out strToken);
                        strToken.tokenSource.Cancel();
                        strToken.tokenSource.Dispose();
                        strToken.tokenSource = new CancellationTokenSource();
                        strToken.token = strToken.tokenSource.Token;
                        audioStreamDict.TryUpdate(client, strToken);
                    }
                    else
                    {
                        strToken.audioStream = client.CreatePCMStream(960);
                        strToken.tokenSource = new CancellationTokenSource();
                        strToken.token = strToken.tokenSource.Token;
                        audioStreamDict.TryAdd(client, strToken);
                    }

                    var output = ffmpeg.StandardOutput.BaseStream; //1920, 2880, 960
                    await output.CopyToAsync(strToken.audioStream, 960, strToken.token).ContinueWith(task =>
                    {
                        if (!task.IsCanceled && task.IsFaulted) //supress cancel exception
                            Console.WriteLine(task.Exception);
                    });
                    ffmpeg.WaitForExit();
                    await strToken.audioStream.FlushAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }*/

        public struct audioStream_Token
        {
            public AudioOutStream audioStream;
            public CancellationTokenSource tokenSource;
            public CancellationToken token;

            public audioStream_Token(AudioOutStream stream, CancellationTokenSource source, CancellationToken _token)
            {
                audioStream = stream;
                tokenSource = source;
                token = _token;
            }
        }

        private void InitializeLoader()
        {
            jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        private void SaveDatabase()
        {
            using (StreamWriter sw = File.CreateText(@"songDatabase.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    jSerializer.Serialize(writer, songDataBase);
                }
            }
        }

        private void LoadDatabse()
        {
            if (File.Exists("songDatabase.json"))
            {
                using (StreamReader sr = File.OpenText(@"songDatabase.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        songDataBase = jSerializer.Deserialize<List<string>>(reader);
                    }
                }
            }
            else
            {
                File.Create("songDatabase.json");
            }
        }
    } // CLASS


    public static class Extensions
    {
        public static bool TryUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key, TValue value)
        {
            TValue currentValue = default(TValue);
            if (self.ContainsKey(key))
                self.TryGetValue(key, out currentValue);
            return self.TryUpdate(key, value, currentValue);
        }

        public static TValue TryGet<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
        {
            TValue value = default(TValue);
            if (self.ContainsKey(key))
                self.TryGetValue(key, out value);
            return value;
        }
    }
}
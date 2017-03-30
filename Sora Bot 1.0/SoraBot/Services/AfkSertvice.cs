using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sora_Bot_1.SoraBot.Services
{
    public class AfkSertvice
    {

        private ConcurrentDictionary<ulong, _afkStruct> _afkDict = new ConcurrentDictionary<ulong, _afkStruct>();
        private JsonSerializer _jSerializer = new JsonSerializer();
        private int _timeAdd = 30000;

        public AfkSertvice()
        {
            InitializeLoader();
            LoadDatabase();
        }

        public async Task ToggleAFK(CommandContext Context, string awayMsg)
        {
            try
            {
                if (!_afkDict.ContainsKey(Context.User.Id))
                {
                    //add
                    if (awayMsg == null)
                        awayMsg = "";
                    _afkStruct str = new _afkStruct
                    {
                        message = awayMsg.Length < 80 ? awayMsg : awayMsg.Substring(0,80)+"..." ,
                        timeToTriggerAgain = DateTime.UtcNow
                    };
                    _afkDict.TryAdd(Context.User.Id, str);
                    await Context.Channel.SendMessageAsync(":white_check_mark: You are now set AFK");
                }
                else
                {
                    //remove
                    _afkStruct ignore;
                    _afkDict.TryRemove(Context.User.Id, out ignore);
                    await Context.Channel.SendMessageAsync(":white_check_mark: AFK has been removed");
                }
                SaveDatabase();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
            
        }

        public async Task Client_MessageReceived(SocketMessage msg)
        {
            try
            {
                if (msg.MentionedUsers.Count < 1)
                {
                    return;
                }
                foreach (var u in msg.MentionedUsers)
                {
                    if (_afkDict.ContainsKey(u.Id))
                    {
                        _afkStruct str = new _afkStruct();
                        _afkDict.TryGetValue(u.Id, out str);
                        //await msg.Channel.SendMessageAsync($"{str.timeToTriggerAgain.CompareTo(DateTime.UtcNow)}");
                        if(str.timeToTriggerAgain.CompareTo(DateTime.UtcNow) >0)
                        {
                            return;
                        }
                        str.timeToTriggerAgain = DateTime.UtcNow.AddSeconds(30);
                        var eb = new EmbedBuilder()
                        {
                            Color = new Color(4, 97, 247),
                            Author = new EmbedAuthorBuilder()
                            {
                                IconUrl = u.GetAvatarUrl(),
                                Name = $"{u.Username} is currently AFK"
                            },
                            Description = str.message
                        };
                        _afkDict.TryUpdate(u.Id, str);
                        await msg.Channel.SendMessageAsync("", false, eb);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
                //await SentryService.SendMessage($"MSG WITH ERROR: \n{msg}");
            }
        }

        private void InitializeLoader()
        {
            _jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            _jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public void SaveDatabase()
        {
            using (StreamWriter sw = File.CreateText(@"afkGlobal.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    _jSerializer.Serialize(writer, _afkDict);
                }
            }
        }

        private void LoadDatabase()
        {
            if (File.Exists("afkGlobal.json"))
            {
                using (StreamReader sr = File.OpenText(@"afkGlobal.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = _jSerializer.Deserialize<ConcurrentDictionary<ulong, _afkStruct>>(reader);
                        if (temp == null)
                            return;
                        _afkDict = temp;
                    }
                }
            }
            else
            {
                File.Create("afkGlobal.json").Dispose();
            }
        }



        public struct _afkStruct
        {
            public string message;
            public DateTime timeToTriggerAgain;
        }

        /*
        private class afkData : IEquatable<T>
        {
            private T value;
            public bool Equals(T other)
            {
                throw new NotImplementedException();
            }
        }*/

    }
}

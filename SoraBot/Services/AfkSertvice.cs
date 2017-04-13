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

        private async Task AddAfk(CommandContext Context, string awayMsg, bool updated)
        {
            if (awayMsg == null)
                awayMsg = "";
            _afkStruct str = new _afkStruct
            {
                message = awayMsg.Length < 80 ? awayMsg : awayMsg.Substring(0, 80) + "...",
                timeToTriggerAgain = DateTime.UtcNow
            };
            _afkDict.AddOrUpdate(Context.User.Id, str, ((key, oldValue) => str));
            await Context.Channel.SendMessageAsync($"{(updated == true ? ":white_check_mark: Your AFK status has been updated" : ":white_check_mark: You are now set AFK")}");
        }

        public async Task ToggleAFK(CommandContext Context, string awayMsg)
        {
            try
            { 
                if (!_afkDict.ContainsKey(Context.User.Id))
                {
                    //add
                    await AddAfk(Context, awayMsg, false);
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(awayMsg))
                    {
                        //remove
                        _afkStruct ignore;
                        _afkDict.TryRemove(Context.User.Id, out ignore);
                        await Context.Channel.SendMessageAsync(":white_check_mark: AFK has been removed");
                    }
                    else
                    {
                        await AddAfk(Context, awayMsg, true);
                    }
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
            if (msg.Author.Id == 270931284489011202 || msg.Author.Id == 276304865934704642)
                return;

            if (msg.MentionedUsers.Count < 1)
                return;

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

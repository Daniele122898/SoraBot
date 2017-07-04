using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sora_Bot_1.SoraBot.Services.Mod;

namespace Sora_Bot_1.SoraBot.Services.UserBlacklist
{
    public static class BlackListDB
    {
        static private JsonSerializer _jSerializer = new JsonSerializer();
        static public void InitializeLoader()
        {
            _jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            _jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        static public void SaveBlackList(ConcurrentDictionary<ulong, List<ulong>> modlogsDict)
        {
            using (StreamWriter sw = File.CreateText(@"BlackListUser.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    _jSerializer.Serialize(writer, modlogsDict);
                }
            }
        }

        static public ConcurrentDictionary<ulong, List<ulong>> LoadBlackList()
        {
            if (File.Exists("BlackListUser.json"))
            {
                using (StreamReader sr = File.OpenText(@"BlackListUser.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = _jSerializer.Deserialize<ConcurrentDictionary<ulong, List<ulong>>>(reader);
                        return temp;
                    }
                }
            }
            else
            {
                File.Create("BlackListUser.json").Dispose();
            }
            return null;
        }
    }
}

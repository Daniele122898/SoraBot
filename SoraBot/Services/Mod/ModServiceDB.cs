using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sora_Bot_1.SoraBot.Services.Mod
{
    public static class ModServiceDB
    {
        static private JsonSerializer _jSerializer = new JsonSerializer();
        static public void InitializeLoader()
        {
            _jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            _jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        static public void SaveModLogs(ConcurrentDictionary<ulong, modLogs> modlogsDict)
        {
            using (StreamWriter sw = File.CreateText(@"ModLogs.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    _jSerializer.Serialize(writer, modlogsDict);
                }
            }
        }

        static public ConcurrentDictionary<ulong, modLogs> LoadModLogs()
        {
            if (File.Exists("ModLogs.json"))
            {
                using (StreamReader sr = File.OpenText(@"ModLogs.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = _jSerializer.Deserialize<ConcurrentDictionary<ulong, modLogs>>(reader);
                        return temp;
                    }
                }
            }
            else
            {
                File.Create("ModLogs.json").Dispose();
            }
            return null;
        }

        static public void SavePunishLogs(ConcurrentDictionary<ulong, punishStruct> punishLogs)
        {
            using (StreamWriter sw = File.CreateText(@"PunishLogs.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    _jSerializer.Serialize(writer, punishLogs);
                }
            }
        }

        static public ConcurrentDictionary<ulong, punishStruct> LoadPunishLogs()
        {
            if (File.Exists("PunishLogs.json"))
            {
                using (StreamReader sr = File.OpenText(@"PunishLogs.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = _jSerializer.Deserialize<ConcurrentDictionary<ulong, punishStruct>>(reader);
                        return temp;
                    }
                }
            }
            else
            {
                File.Create("PunishLogs.json").Dispose();
            }
            return null;
        }
    }
}

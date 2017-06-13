using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sora_Bot_1.SoraBot.Services.Marry;

namespace Sora_Bot_1.SoraBot.Services.GlobalSoraBans
{
    public class GlobalBanDB
    {
        private static GlobalBanDB _instance;

        private GlobalBanDB()
        {
        }

        public static GlobalBanDB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalBanDB();
                }
                return _instance;
            }
        }
        
        private JsonSerializer _jSerializer = new JsonSerializer();
        
        public void InitializeLoader()
        {
            _jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            _jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }
        
        public void SaveGlobalBanData(ConcurrentDictionary<ulong, string> globalBanDict)
        {
            using (StreamWriter sw = File.CreateText(@"GlobalBans.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    _jSerializer.Serialize(writer, globalBanDict);
                }
            }
        }

        public ConcurrentDictionary<ulong, string> LoadGlobalBanData()
        {
            if (File.Exists("GlobalBans.json"))
            {
                using (StreamReader sr = File.OpenText(@"GlobalBans.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = _jSerializer.Deserialize<ConcurrentDictionary<ulong, string>>(reader);
                        if(temp!= null)
                            return temp;
                        return new ConcurrentDictionary<ulong, string>();
                    }
                }
            }
            else
            {
                File.Create("GlobalBans.json").Dispose();
            }
            return new ConcurrentDictionary<ulong, string>();
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sora_Bot_1.SoraBot.Services.Mod;

namespace Sora_Bot_1.SoraBot.Services.Marry
{
    public class MarryServiceDB
    {
        private static MarryServiceDB _instance;

        private MarryServiceDB()
        {
        }

        public static MarryServiceDB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MarryServiceDB();
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
        
        public void SaveMarryData(ConcurrentDictionary<ulong, List<MarryData>> marryDict)
        {
            using (StreamWriter sw = File.CreateText(@"MarryData.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    _jSerializer.Serialize(writer, marryDict);
                }
            }
        }

        public ConcurrentDictionary<ulong, List<MarryData>> LoadMarryData()
        {
            if (File.Exists("MarryData.json"))
            {
                using (StreamReader sr = File.OpenText(@"MarryData.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = _jSerializer.Deserialize<ConcurrentDictionary<ulong, List<MarryData>>>(reader);
                        if(temp!= null)
                            return temp;
                        return new ConcurrentDictionary<ulong, List<MarryData>>();
                    }
                }
            }
            else
            {
                File.Create("MarryData.json").Dispose();
            }
            return new ConcurrentDictionary<ulong, List<MarryData>>();
        }

    }
    
    public class MarryData
    {
        public ulong UserId { get; set; }
        public DateTime MarriedSince { get; set; }
    }
}
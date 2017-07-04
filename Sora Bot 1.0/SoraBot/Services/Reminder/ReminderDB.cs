using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sora_Bot_1.SoraBot.Services.Reminder
{
    public static class ReminderDB
    {

        static private JsonSerializer _jSerializer = new JsonSerializer();

        static public void InitializeLoader()
        {
            _jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            _jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        static public void SaveReminders(ConcurrentDictionary<ulong, List<RemindData>> modlogsDict)
        {
            using (StreamWriter sw = File.CreateText(@"Reminders.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    _jSerializer.Serialize(writer, modlogsDict);
                }
            }
        }

        static public ConcurrentDictionary<ulong, List<RemindData>> LoadReminders()
        {
            if (File.Exists("Reminders.json"))
            {
                using (StreamReader sr = File.OpenText(@"Reminders.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = _jSerializer.Deserialize<ConcurrentDictionary<ulong, List<RemindData>>>(reader);
                        return temp;
                    }
                }
            }
            else
            {
                File.Create("Reminders.json").Dispose();
            }
            return null;
        }
    }
}

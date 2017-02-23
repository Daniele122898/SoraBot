using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Services.ChangelogService
{
    public static class ChangelogService
    {
        public static string changelog { get; private set; }

        public static void LoadChangelog()
        {
            changelog = File.ReadAllText("CHANGELOG");
        }

        public static async Task modifyChangelog(string newChangelog)
        {
            changelog = newChangelog;
        }
    }
}

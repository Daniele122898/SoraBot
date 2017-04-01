using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sora_Bot_1.SoraBot.Services.PatService
{

    public enum affinityType
    {
        pat, hug, kiss, poke, slap
    };

    public class PatService
    {
        private ConcurrentDictionary<ulong, int> patDict = new ConcurrentDictionary<ulong, int>();
        private ConcurrentDictionary<ulong, AffinityStats> affinityDict = new ConcurrentDictionary<ulong, AffinityStats>();
        private JsonSerializer jSerializer = new JsonSerializer();

        public PatService()
        {
            InitializeLoader();
            LoadDatabase();
        }

        public async Task AddPat(IUser user, CommandContext context)
        {
            try
            {
                if (patDict.ContainsKey(user.Id))
                {
                    int counter = 0;
                    patDict.TryGetValue(user.Id, out counter);
                    counter++;
                    patDict.TryUpdate(user.Id, counter);
                }
                else
                {
                    patDict.TryAdd(user.Id, 1);
                }
                SaveDatabase();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
              
        }

        public async Task ChangeAffinity(affinityType type, IUser user, CommandContext Context)
        {
            try
            {
                AffinityStats stats = new AffinityStats();
                if (affinityDict.ContainsKey(user.Id))
                {
                    affinityDict.TryGetValue(user.Id, out stats);
                }
                switch (type)
                {
                    case (affinityType.pat):
                        stats.pats++;
                        break;
                    case (affinityType.hug):
                        stats.hugs++;
                        break;
                    case (affinityType.kiss):
                        stats.kisses++;
                        break;
                    case (affinityType.poke):
                        stats.pokes++;
                        break;
                    case (affinityType.slap):
                        stats.slaps++;
                        break;
                    default:
                        await Context.Channel.SendMessageAsync(":no_entry_sign: Something went horribly wrong :eyes:");
                        await SentryService.SendMessage("AFFINITY FAILED IN SWITCH");
                        break;
                }

                affinityDict.AddOrUpdate(
                    user.Id,
                    stats,
                    (key, oldValue) => stats);
                //TODO SAVE DATABASE
                SaveDatabase();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task GetAffinity(IUser user, CommandContext Context)
        {
            try
            {
                if (!affinityDict.ContainsKey(user.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: User did not receive any pats,hugs,kisses,pokes or slaps yet!");
                    return;
                }
                AffinityStats stats = new AffinityStats();
                affinityDict.TryGetValue(user.Id, out stats);
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Title = $"Affinity stats of {user.Username}#{user.Discriminator}",
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Description = $"Pats:\t{stats.pats}\n" +
                    $"Hugs:\t{stats.hugs}\n" +
                    $"Kisses:\t{stats.kisses}\n" +
                    $"Pokes:\t{stats.pokes}\n" +
                    $"Slaps:\t{stats.slaps}\n" +
                    $"Affinity:\t{stats.GetAffinity()}/100"
                };

                await Context.Channel.SendMessageAsync("",false,eb);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task CheckPats(IUser user, CommandContext Context)
        {
            try
            {
                if (patDict.ContainsKey(user.Id))
                {
                    int counter = 0;
                    patDict.TryGetValue(user.Id, out counter);
                    await Context.Channel.SendMessageAsync($"{user.Mention} has received a total of {counter} pats (◕‿◕✿)");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{user.Mention} has not received any pats yet (⌯˃̶᷄ ﹏ ˂̶᷄⌯)ﾟ be the first!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
            
        }

        public struct AffinityStruct
        {
            public int pats;
            public int hugs;
            public int kisses;
            public int pokes;
            public int slaps;
            public float affinity;
        }

        private void InitializeLoader()
        {
            jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public void SaveDatabase()
        {
            using (StreamWriter sw = File.CreateText(@"Pats.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    jSerializer.Serialize(writer, affinityDict);
                }
            }
        }

        private void LoadDatabase()
        {
            if (File.Exists("Pats.json"))
            {
                using (StreamReader sr = File.OpenText(@"Pats.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = jSerializer.Deserialize<ConcurrentDictionary<ulong, AffinityStats>>(reader);
                        if (temp == null)
                            return;
                        affinityDict = temp;
                    }
                }
            }
            else
            {
                File.Create("Pats.json").Dispose();
            }
        }
    }//CLASS

    public class AffinityStats
    {
        public int pats = 0;
        public int hugs=0;
        public int kisses=0;
        public int pokes=0;
        public int slaps=0;
        public double GetAffinity()
        {
            double total = pats + hugs * 2 + kisses * 3 + slaps;
            double good = pats + hugs * 2 + kisses * 3;
            if (total == 0)
                return 0;
            if (good == 0)
                return 0;
            return Math.Round((100.0 / total * good), 2);
        }
        
        }
}

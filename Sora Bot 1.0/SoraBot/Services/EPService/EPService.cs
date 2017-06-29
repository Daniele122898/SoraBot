using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using SixLabors.Primitives; 

namespace Sora_Bot_1.SoraBot.Services.EPService
{
    public class EPService
    {
        ConcurrentDictionary<ulong, userStruct> userEPDict = new ConcurrentDictionary<ulong, userStruct>();
        ConcurrentDictionary<ulong, bool> userBG = new ConcurrentDictionary<ulong, bool>();
        ConcurrentDictionary<ulong, DateTime> userCooldown = new ConcurrentDictionary<ulong, DateTime>();
        ConcurrentDictionary<ulong, DateTime> userBGUpdateCD = new ConcurrentDictionary<ulong, DateTime>();
        private DiscordSocketClient client;
        private JsonSerializer jSerializer = new JsonSerializer();
        private int timeToUpdate = Environment.TickCount + 30000;
        private List<ulong> lvlSubsriberList = new List<ulong>();


        public EPService(DiscordSocketClient c)
        {
            client = c;
            InitializeLoader();
            LoadDatabase();
            LoadDatabaseGuild();
            LoadDatabaseBG();

            ProfileImageProcessing.Initialize();
        }

        public async Task SetBG(string url, SocketCommandContext Context)
        {
            try
            {

                userStruct str = new userStruct();
                if (userEPDict.ContainsKey(Context.User.Id))
                {
                    userEPDict.TryGetValue(Context.User.Id, out str);
                    if (str.level < 20)
                    {
                        await Context.Channel.SendMessageAsync(":no_entry_sign: You must be level 20 to unlock custom BGs!");
                        return;
                    }
                }
                if (String.IsNullOrEmpty(url))
                {
                    bool ig = false;
                    if (userBG.TryRemove(Context.User.Id, out ig))
                    {
                        await Context.Channel.SendMessageAsync(
                            ":white_check_mark: Removed custom Background and reverted to defualt card!");
                        if (File.Exists($"{Context.User.Id}BGF.jpg"))
                        {
                            File.Delete($"{Context.User.Id}BGF.jpg");
                        }
                        SaveDatabaseBG();
                        return;
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync(
                            ":no_entry_sign: User already had no BG so there is nothing to remove!");
                        if (File.Exists($"{Context.User.Id}BGF.jpg"))
                        {
                            File.Delete($"{Context.User.Id}BGF.jpg");
                        }
                        SaveDatabaseBG();
                        return;
                    }
                }

                if (!url.EndsWith(".jpg") && !url.EndsWith(".png") && !url.EndsWith(".gif") && !url.EndsWith(".jpeg"))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You must link an Image!");
                    return;
                }

                if (Context.User.Id != 192750776005689344 && userBGUpdateCD.ContainsKey(Context.User.Id))
                {
                    DateTime timeToTriggerAgain;
                    userBGUpdateCD.TryGetValue(Context.User.Id, out timeToTriggerAgain);
                    if (timeToTriggerAgain.CompareTo(DateTime.UtcNow) < 0)
                    {
                        timeToTriggerAgain = DateTime.UtcNow.AddSeconds(45);
                        userBGUpdateCD.TryUpdate(Context.User.Id, timeToTriggerAgain);
                    }
                    else
                    {
                        var time = timeToTriggerAgain.Subtract(DateTime.UtcNow.TimeOfDay);
                        int remainingTime = time.Second;
                        await Context.Channel.SendMessageAsync(
                            $":no_entry_sign: You are still on cooldown! Wait another {remainingTime} seconds!");
                        return;
                    }
                }
                else
                {
                    DateTime timeToTriggerAgain = DateTime.UtcNow.AddSeconds(45);
                    userBGUpdateCD.TryAdd(Context.User.Id, timeToTriggerAgain);
                }

                Uri requestUri = new Uri(url);

                if (File.Exists($"{Context.User.Id}BGF.jpg"))
                {
                    File.Delete($"{Context.User.Id}BGF.jpg");
                }

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                using (
                    Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream($"{Context.User.Id}BG.jpg",
                            FileMode.Create, FileAccess.Write,
                            FileShare.None, 3145728, true))
                {
                    await contentStream.CopyToAsync(stream);
                    await contentStream.FlushAsync();
                    contentStream.Dispose();
                    await stream.FlushAsync();
                    stream.Dispose();
                    Console.WriteLine("DONE BG STREAM");
                }

                Configuration.Default.AddImageFormat(new PngFormat());

                using (var input = ImageSharp.Image.Load($"{Context.User.Id}BG.jpg"))
                {
                    //using (var output = File.OpenWrite($"{Context.User.Id}BGF.jpg"))
                    //{
                        //var image = new ImageSharp.Image(input);
                        //int divide = image.Width / 900;
                        //int width = image.Width / divide;
                        //int height = image.Height / divide;
                        input.Resize(new ResizeOptions
                        {
                            Size = new Size(900, 500),
                            Mode = ResizeMode.Crop
                        });
                        //image.ExifProfile = null; TODO FIX THIS
                        //image.Quality = quality;
                        input.Save($"{Context.User.Id}BGF.png");
                        input.Dispose();
                        input.Dispose();
                        //await output.FlushAsync();
                        //output.Dispose();

                        /*.Resize(new ResizeOptions
                            {
                                Size = new ImageSharp.Size(size, size),
                                Mode = ResizeMode.Max
                            });*/
                    //}
                }
                //IMAGE RESIZE END
                if (File.Exists($"{Context.User.Id}BG.jpg"))
                {
                    File.Delete($"{Context.User.Id}BG.jpg");
                }


                if (userBG.ContainsKey(Context.User.Id))
                {
                }
                else
                {
                    userBG.TryAdd(Context.User.Id, true);
                }
                SaveDatabaseBG();

                await Context.Channel.SendMessageAsync(":white_check_mark: Successfully set new BG!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
                await Context.Channel.SendMessageAsync(":no_entry_sign: Failed to Download Image! Take another one, sorry :persevere:");
            }
        }

        public async Task shotTop10(SocketCommandContext Context)
        {
            try
            {

                //GET RANK

                var guild = ((SocketGuild)Context.Guild);
                //guild.DownloadUsersAsync();

                if (guild.MemberCount < 200)
                {
                    guild.DownloadUsersAsync().Wait();
                    //await guild.DownloadUsersAsync();
                }

                //FEED LIST
                Dictionary<ulong, float> epList = new Dictionary<ulong, float>();
                foreach (var u in guild.Users)
                {
                    if (!u.IsBot && userEPDict.ContainsKey(u.Id))
                    {
                        userStruct str = new userStruct();
                        userEPDict.TryGetValue(u.Id, out str);
                        if (!epList.ContainsKey(u.Id))
                        {
                            epList.Add(u.Id, str.ep);
                        }
                    }
                }
                /*
                //GETLIST
                var sortedList = epList.OrderByDescending(pair => pair.Value)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                var rank = GetIndex(sortedList, userInfo.Id) + 1;
                //END RANK

                //FEED LIST
                Dictionary<string, float> epList = new Dictionary<string, float>();
                foreach (var u in guild.Users)
                {
                    if (!u.IsBot && userEPDict.ContainsKey(u.Id))
                    {
                        userStruct str = new userStruct();
                        userEPDict.TryGetValue(u.Id, out str);
                        if (!epList.ContainsKey($"{u.Username}#{u.Discriminator}"))
                        {
                            epList.Add($"{u.Username}#{u.Discriminator}", str.ep);
                        }
                    }
                }*/

                //GETLIST
                var sortedList = epList.OrderByDescending(pair => pair.Value)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                var top10 = sortedList.Take(10);
                /* TURN IT BACK INTO DICT
                 * var top5 = dict.OrderByDescending(pair => pair.Value).Take(5)
               .ToDictionary(pair => pair.Key, pair => pair.Value);
               */

                //CREATE TOP 10
                var eb = new EmbedBuilder()
                {
                    Color = new Discord.Color(4, 97, 247),
                    ThumbnailUrl = new Uri(Context.Guild.IconUrl),
                    Title = $"Top 10 in {guild.Name} (Global EP)",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                        IconUrl = new Uri(Context.User.GetAvatarUrl())
                    }
                };
                int rank = 1;
                foreach (var u in top10)
                {
                    int level = (int) Math.Round(0.15F * Math.Sqrt(u.Value));
                    var us = guild.GetUser(u.Key);
                    eb.AddField((x) =>
                    {
                        x.Name = $"{rank}. {us.Username}#{us.Discriminator}";
                        x.IsInline = false;
                        x.Value = $"Lvl. {level} \tEP: {u.Value}";
                    });
                    rank++;
                }
                int index = GetIndex(sortedList, Context.User.Id);
                int lvl =
                    (int)
                    Math.Round(0.15F * Math.Sqrt(sortedList[Context.User.Id]));
                eb.AddField((x) =>
                {
                    x.Name = $"Your Rank: {index + 1}";
                    x.IsInline = false;
                    x.Value =
                        $"Level: {lvl} \tEP: {sortedList[Context.User.Id]}";
                });
                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public static int GetIndex(Dictionary<ulong, float> dictionary, ulong key)
        {
            for (int index = 0; index < dictionary.Count; index++)
            {
                if (dictionary.Skip(index).First().Key == key)
                    return index;
            }

            return -1;
        }

        public async Task ShowProfile(SocketCommandContext Context, IUser user)
        {
            try
            {
                if (Context.User.Id != 192750776005689344 && userCooldown.ContainsKey(Context.User.Id))
                {
                    DateTime timeToTriggerAgain;
                    userCooldown.TryGetValue(Context.User.Id, out timeToTriggerAgain);
                    if (timeToTriggerAgain.CompareTo(DateTime.UtcNow) < 0)
                    {
                        timeToTriggerAgain = DateTime.UtcNow.AddSeconds(30);
                        userCooldown.TryUpdate(Context.User.Id, timeToTriggerAgain);
                    }
                    else
                    {
                        var time = timeToTriggerAgain.Subtract(DateTime.UtcNow.TimeOfDay);
                        int remainingTime = time.Second;
                        await Context.Channel.SendMessageAsync(
                            $":no_entry_sign: You are still on cooldown! Wait another {remainingTime} seconds!");
                        return;
                    }
                }
                else
                {
                    DateTime timeToTriggerAgain = DateTime.UtcNow.AddSeconds(30);
                    userCooldown.TryAdd(Context.User.Id, timeToTriggerAgain);
                }
                if (userBG.ContainsKey(user.Id))
                {
                    //await DrawText(user.GetAvatarUrl(), user, Context);
                    await DrawProfileWithBG(user.GetAvatarUrl(), user, Context);
                }
                else
                {
                    //await DrawText2(user.GetAvatarUrl(), user, Context);
                    await DrawProfile(user.GetAvatarUrl(), user, Context);
                }
                //await Context.Channel.SendMessageAsync($"Image \n{img}");
                if (File.Exists($"{user.Id}.png"))
                {
                    await Context.Channel.SendFileAsync($"{user.Id}.png", null, false, null);
                    File.Delete($"{user.Id}.png");
                    File.Delete($"{user.Id}Avatar.png");
                    File.Delete($"{user.Id}AvatarF.png");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Failed to create Image! This may be due to the Image you linked is damaged or unsupported. Try a new Custom Pic or use the default Image (p setbg with no parameter sets it to the default image)");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        private async Task DrawProfileWithBG(String AvatarUrl, IUser userInfo, SocketCommandContext Context)
        {
            if (String.IsNullOrEmpty(AvatarUrl))
                AvatarUrl =
                    "http://is2.mzstatic.com/image/pf/us/r30/Purple7/v4/89/51/05/89510540-66df-9f6f-5c91-afa5e48af4e8/mzl.sbwqpbfh.png";

            Uri requestUri = new Uri(AvatarUrl);

            if (File.Exists($"{userInfo.Id}Avatar.png"))
            {
                File.Delete($"{userInfo.Id}Avatar.png");
            }

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (
                Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream($"{userInfo.Id}Avatar.png", FileMode.Create, FileAccess.Write,
                        FileShare.None, 3145728, true))
            {
                await contentStream.CopyToAsync(stream);
                await contentStream.FlushAsync();
                contentStream.Dispose();
                await stream.FlushAsync();
                stream.Dispose();
                Console.WriteLine("DONE STREAM");
            }

            var username = userInfo.Username;
            if (userInfo.Username.Length > 20)
            {
                username = userInfo.Username.Remove(20) + "...";
            }



            //GET RANK

            var guild = ((SocketGuild)Context.Guild);
            //guild.DownloadUsersAsync();

            if (guild.MemberCount < 200)
            {
                guild.DownloadUsersAsync().Wait();
                //await guild.DownloadUsersAsync();
            }

            //FEED LIST
            Dictionary<ulong, float> epList = new Dictionary<ulong, float>();
            foreach (var u in guild.Users)
            {
                if (!u.IsBot && userEPDict.ContainsKey(u.Id))
                {
                    userStruct str = new userStruct();
                    userEPDict.TryGetValue(u.Id, out str);
                    if (!epList.ContainsKey(u.Id))
                    {
                        epList.Add(u.Id, str.ep);
                    }
                }
            }

            //GETLIST
            var sortedList = epList.OrderByDescending(pair => pair.Value)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            var rank = GetIndex(sortedList, userInfo.Id) + 1;
            //END RANK

            userStruct user = new userStruct();
            if (userEPDict.ContainsKey(userInfo.Id))
            {
                userEPDict.TryGetValue(userInfo.Id, out user);
            }
            else
            {
                user.ep = 0;
                user.level = 0;
            }
            //level = constant * sqrt(XP)

            ProfileImageProcessing.GenerateProfileWithBg($"{userInfo.Id}Avatar.png", $"{userInfo.Id}BGF.png", username, rank,user.level, (int)user.ep, $"{userInfo.Id}.png");
        }

        private async Task DrawProfile(String AvatarUrl, IUser userInfo, SocketCommandContext Context)
        {
            if (String.IsNullOrEmpty(AvatarUrl))
                AvatarUrl =
                    "http://is2.mzstatic.com/image/pf/us/r30/Purple7/v4/89/51/05/89510540-66df-9f6f-5c91-afa5e48af4e8/mzl.sbwqpbfh.png";

            Uri requestUri = new Uri(AvatarUrl);

            if (File.Exists($"{userInfo.Id}Avatar.png"))
            {
                File.Delete($"{userInfo.Id}Avatar.png");
            }

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (
                Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream($"{userInfo.Id}Avatar.png", FileMode.Create, FileAccess.Write,
                        FileShare.None, 3145728, true))
            {
                await contentStream.CopyToAsync(stream);
                await contentStream.FlushAsync();
                contentStream.Dispose();
                await stream.FlushAsync();
                stream.Dispose();
                Console.WriteLine("DONE STREAM");
            }

            var username = userInfo.Username;
            if (userInfo.Username.Length > 20)
            {
                username = userInfo.Username.Remove(20) + "...";
            }



            //GET RANK

            var guild = ((SocketGuild)Context.Guild);
            //guild.DownloadUsersAsync();

            if (guild.MemberCount < 200)
            {
                guild.DownloadUsersAsync().Wait();
                //await guild.DownloadUsersAsync();
            }

            //FEED LIST
            Dictionary<ulong, float> epList = new Dictionary<ulong, float>();
            foreach (var u in guild.Users)
            {
                if (!u.IsBot && userEPDict.ContainsKey(u.Id))
                {
                    userStruct str = new userStruct();
                    userEPDict.TryGetValue(u.Id, out str);
                    if (!epList.ContainsKey(u.Id))
                    {
                        epList.Add(u.Id, str.ep);
                    }
                }
            }

            //GETLIST
            var sortedList = epList.OrderByDescending(pair => pair.Value)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            var rank = GetIndex(sortedList, userInfo.Id) + 1;
            //END RANK

            userStruct user = new userStruct();
            if (userEPDict.ContainsKey(userInfo.Id))
            {
                userEPDict.TryGetValue(userInfo.Id, out user);
            }
            else
            {
                user.ep = 0;
                user.level = 0;
            }
            
            ProfileImageProcessing.GenerateProfile($"{userInfo.Id}Avatar.png", username, rank, user.level, (int)user.ep, $"{userInfo.Id}.png");
        }

        public async Task ToggleEPSubscribe(SocketCommandContext context)
        {
            if (lvlSubsriberList.Contains(context.User.Id))
            {
                lvlSubsriberList.Remove(context.User.Id);
                await context.Channel.SendMessageAsync(":white_check_mark: You wont be notified on level up anymore!");
            }
            else
            {
                lvlSubsriberList.Add(context.User.Id);
                await context.Channel.SendMessageAsync(":white_check_mark: You will be notified on level ups!");
            }
            SaveDatabaseGuild();
        }

        public async Task IncreaseEP(SocketMessage msg)
        {
            //Don't process the comand if it was a system message
            var message = msg as SocketUserMessage;
            if (message == null) return;


            //Create a command Context
            var context = new SocketCommandContext(client, message);
            if (context.IsPrivate)
                return;
            if (context.User.IsBot)
                return;
            try
            {
                userStruct user = new userStruct();
                if (userEPDict.ContainsKey(context.User.Id))
                {
                    userEPDict.TryGetValue(context.User.Id, out user);

                    if (user.CanGainAgain == default(DateTime))
                        user.CanGainAgain = DateTime.UtcNow;

                    if (user.CanGainAgain.CompareTo(DateTime.UtcNow) > 0)
                        return;

                    user.CanGainAgain = DateTime.UtcNow.AddSeconds(10);
                    

                    int previousLvl = user.level;
                    user.ep += CalculateEP(context);
                    user.level = (int) Math.Round(0.15F * Math.Sqrt(user.ep));
                    if (previousLvl != user.level)
                    {
                        if (lvlSubsriberList.Contains(context.User.Id))
                        {
                            await (await context.User.GetOrCreateDMChannelAsync()).SendMessageAsync(
                                $":trophy: You leveled up! You are now level **{user.level}** \\ (•◡•) /");
                        }
                    }
                    userEPDict.TryUpdate(context.User.Id, user);
                }
                else
                {
                    user.ep += CalculateEP(context);
                    user.level = (int) Math.Round(0.15F * Math.Sqrt(user.ep));
                    userEPDict.TryAdd(context.User.Id, user);
                }
                if (Environment.TickCount >= timeToUpdate)
                {
                    timeToUpdate = Environment.TickCount + 30000;
                    SaveDatabase();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, context);
            }
        }

        private float CalculateEP(SocketCommandContext context)
        {
            int lenght = (int) Math.Round(context.Message.Content.Length / 10F);
            return lenght;
        }

        private struct userStruct
        {
            public float ep;
            public int level;
            public DateTime CanGainAgain;
        }

        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private void InitializeLoader()
        {
            jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public void SaveDatabaseGuild()
        {
            using (StreamWriter sw = File.CreateText(@"UserEPSubscriber.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    jSerializer.Serialize(writer, lvlSubsriberList);
                }
            }
        }

        public void SaveDatabaseBG()
        {
            using (StreamWriter sw = File.CreateText(@"UserCustomBG.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    jSerializer.Serialize(writer, userBG);
                }
            }
        }

        private void LoadDatabaseBG()
        {
            if (File.Exists("UserCustomBG.json"))
            {
                using (StreamReader sr = File.OpenText(@"UserCustomBG.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var userBGTemp = jSerializer.Deserialize<ConcurrentDictionary<ulong, bool>>(reader);
                        if (userBGTemp == null)
                            return;
                        userBG = userBGTemp;
                    }
                }
            }
            else
            {
                File.Create("UserCustomBG.json").Dispose();
            }
        }

        private void LoadDatabaseGuild()
        {
            if (File.Exists("UserEPSubscriber.json"))
            {
                using (StreamReader sr = File.OpenText(@"UserEPSubscriber.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var blackListguildTemp = jSerializer.Deserialize<List<ulong>>(reader);
                        if (blackListguildTemp == null)
                            return;
                        lvlSubsriberList = blackListguildTemp;
                    }
                }
            }
            else
            {
                File.Create("UserEPSubscriber.json").Dispose();
            }
        }

        public void SaveDatabase()
        {
            using (StreamWriter sw = File.CreateText(@"UserEP.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    jSerializer.Serialize(writer, userEPDict);
                }
            }
        }

        private void LoadDatabase()
        {
            if (File.Exists("UserEP.json"))
            {
                using (StreamReader sr = File.OpenText(@"UserEP.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var userEPDictTemp = jSerializer.Deserialize<ConcurrentDictionary<ulong, userStruct>>(reader);
                        if (userEPDictTemp == null)
                            return;
                        userEPDict = userEPDictTemp;
                    }
                }
            }
            else
            {
                File.Create("UserEP.json").Dispose();
            }
        }

        public int GetUserCount()
        {
            return userEPDict.Count;
        }
    }
}
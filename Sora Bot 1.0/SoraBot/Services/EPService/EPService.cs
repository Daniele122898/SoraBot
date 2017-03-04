using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
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
using Size = System.Drawing.Size;

namespace Sora_Bot_1.SoraBot.Services.EPService
{
    public class EPService
    {
        ConcurrentDictionary<ulong, userStruct> userEPDict = new ConcurrentDictionary<ulong, userStruct>();
        ConcurrentDictionary<ulong, bool> userBG = new ConcurrentDictionary<ulong, bool>();
        ConcurrentDictionary<ulong, int> userCooldown = new ConcurrentDictionary<ulong, int>();
        ConcurrentDictionary<ulong, int> userBGUpdateCD = new ConcurrentDictionary<ulong, int>();
        private DiscordSocketClient client;
        private JsonSerializer jSerializer = new JsonSerializer();
        private int timeToUpdate = Environment.TickCount + 30000;
        private List<ulong> lvlSubsriberList = new List<ulong>();
        private int profileX = 26;
        private int profileY = 15;
        private int profileSIZE = 121;

        private int profileX1 = 73;
        private int profileY1 = 273;
        private int profileSIZE1 = 155;


        public EPService(DiscordSocketClient c)
        {
            client = c;
            InitializeLoader();
            LoadDatabase();
            LoadDatabaseGuild();
        }

        public async Task SetBG(string url, CommandContext Context)
        {
            try
            {
                if (String.IsNullOrEmpty(url))
                {
                    bool ig = false;
                    if (userBG.TryRemove(Context.User.Id, out ig))
                    {
                        await Context.Channel.SendMessageAsync(
                            ":white_check_mark: Removed custom Background and reverted to defualt card!");
                        if (File.Exists($"{Context.User.Username}#{Context.User.Discriminator}BGF.jpg"))
                        {
                            File.Delete($"{Context.User.Username}#{Context.User.Discriminator}BGF.jpg");
                        }
                        return;
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync(
                            ":no_entry_sign: User already had no BG so there is nothing to remove!");
                        if (File.Exists($"{Context.User.Username}#{Context.User.Discriminator}BGF.jpg"))
                        {
                            File.Delete($"{Context.User.Username}#{Context.User.Discriminator}BGF.jpg");
                        }
                        return;
                    }
                }
                if (Context.User.Id != 192750776005689344 && userCooldown.ContainsKey(Context.User.Id))
                {
                    int cooldown = 0;
                    userBGUpdateCD.TryGetValue(Context.User.Id, out cooldown);
                    if (Environment.TickCount >= cooldown)
                    {
                        cooldown = Environment.TickCount + 45000;
                        userBGUpdateCD.TryUpdate(Context.User.Id, cooldown);
                    }
                    else
                    {
                        float time = (cooldown - Environment.TickCount) / 1000;
                        int remainingTime = (int)Math.Round(time);
                        await Context.Channel.SendMessageAsync(
                            $":no_entry_sign: You are still on cooldown! Wait another {remainingTime} seconds!");
                        return;
                    }
                }
                else
                {
                    var cooldown = Environment.TickCount + 45000;
                    userBGUpdateCD.TryAdd(Context.User.Id, cooldown);
                }

                Uri requestUri = new Uri(url);

                if (File.Exists($"{Context.User.Username}#{Context.User.Discriminator}BGF.jpg"))
                {
                    File.Delete($"{Context.User.Username}#{Context.User.Discriminator}BGF.jpg");
                }

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                using (
                    Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream($"{Context.User.Username}#{Context.User.Discriminator}BG.jpg", FileMode.Create, FileAccess.Write,
                            FileShare.None, 3145728, true))
                {
                    await contentStream.CopyToAsync(stream);
                    await contentStream.FlushAsync();
                    contentStream.Dispose();
                    await stream.FlushAsync();
                    stream.Dispose();
                    Console.WriteLine("DONE BG STREAM");
                }

                //IMAGE RESIZE
                int size = profileSIZE;

                Configuration.Default.AddImageFormat(new PngFormat());

                using (var input = File.OpenRead($"{Context.User.Username}#{Context.User.Discriminator}BG.jpg"))
                {
                    using (var output = File.OpenWrite($"{Context.User.Username}#{Context.User.Discriminator}BGF.jpg"))
                    {
                        var image = new ImageSharp.Image(input);
                        //int divide = image.Width / 900;
                        //int width = image.Width / divide;
                        //int height = image.Height / divide;
                        image.Resize(new ResizeOptions
                        {
                            Size = new ImageSharp.Size(900, 10000),
                            Mode = ResizeMode.Max
                        });
                        //image.ExifProfile = null; TODO FIX THIS
                        //image.Quality = quality;
                        image.Save(output);
                        image.Dispose();
                        await input.FlushAsync();
                        input.Dispose();
                        await output.FlushAsync();
                        output.Dispose();

                        /*.Resize(new ResizeOptions
                            {
                                Size = new ImageSharp.Size(size, size),
                                Mode = ResizeMode.Max
                            });*/
                    }
                }
                //IMAGE RESIZE END
                if (File.Exists($"{Context.User.Username}#{Context.User.Discriminator}BG.jpg"))
                {
                    File.Delete($"{Context.User.Username}#{Context.User.Discriminator}BG.jpg");
                }
                

                if (userBG.ContainsKey(Context.User.Id))
                {
                }
                else
                {
                    userBG.TryAdd(Context.User.Id, true);
                }
                await Context.Channel.SendMessageAsync(":white_check_mark: Successfully set new BG!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task changeProfileCord(string cords, CommandContext context)
        {
            try
            {
                string[] cordS = cords.Split(' ');
                if (cordS.Length != 2)
                {
                    await context.Channel.SendMessageAsync(":no_entry_sign: Coordinates have to be 2 integers => x y");
                    return;
                }
                /*
                    var coolInput = "abcd123!!!E$%$§&$%";
                    var sanitized = Regex.Replace(coolInput, @"\D", "");
                    cordS[1].Any(char.IsDigit)
                */
                        int x;
                int y;
                if (Int32.TryParse(cordS[0], NumberStyles.Integer, new NumberFormatInfo(), out x) &&
                    Int32.TryParse(cordS[1], NumberStyles.Integer, new NumberFormatInfo(), out y))
                {
                    profileX1 = x;
                    profileY1 = y;
                    await context.Channel.SendMessageAsync($":white_check_mark: Set coordinates to {x} {y}");
                    return;
                }
                else
                {
                    await context.Channel.SendMessageAsync(":no_entry_sign: Coordinates have to be 2 integers => x y");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, context);
            }
        }

        public async Task size(string sizeS, CommandContext context)
        {
            try
            {
                int size;
                if (Int32.TryParse(sizeS, NumberStyles.Integer, new NumberFormatInfo(), out size))
                {
                    profileSIZE1 = size;
                    await context.Channel.SendMessageAsync($":white_check_mark: Size set to {size}");
                }
                else
                {
                    await context.Channel.SendMessageAsync(":no_entry_sign: Size has to be an integer");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, context);
            }
        }

        public async Task shotTop10(CommandContext Context)
        {
            try
            {
                var guild = ((SocketGuild) Context.Guild);
                //guild.DownloadUsersAsync();

                if (guild.MemberCount < 200)
                {
                    guild.DownloadUsersAsync().Wait();
                    //await guild.DownloadUsersAsync();
                }

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
                }

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
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Title = $"Top 10 in {guild.Name} (Global EP)",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                        IconUrl = Context.User.GetAvatarUrl()
                    }
                };
                int rank = 1;
                foreach (var u in top10)
                {
                    int level = (int) Math.Round(0.15F * Math.Sqrt(u.Value));
                    eb.AddField((x) =>
                    {
                        x.Name = $"{rank}. {u.Key}";
                        x.IsInline = false;
                        x.Value = $"Lvl. {level} \tEP: {u.Value}";
                    });
                    rank++;
                }
                int index = GetIndex(sortedList, $"{Context.User.Username}#{Context.User.Discriminator}");
                int lvl =
                    (int)
                    Math.Round(0.15F * Math.Sqrt(sortedList[$"{Context.User.Username}#{Context.User.Discriminator}"]));
                eb.AddField((x) =>
                {
                    x.Name = $"Your Rank: {index + 1}";
                    x.IsInline = false;
                    x.Value =
                        $"Level: {lvl} \tEP: {sortedList[$"{Context.User.Username}#{Context.User.Discriminator}"]}";
                });
                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public static int GetIndex(Dictionary<string, float> dictionary, string key)
        {
            for (int index = 0; index < dictionary.Count; index++)
            {
                if (dictionary.Skip(index).First().Key == key)
                    return index;
            }

            return -1;
        }

        public async Task ShowProfile(CommandContext Context, IUser user)
        {
            try
            {
                if (Context.User.Id != 192750776005689344 && userCooldown.ContainsKey(Context.User.Id))
                {
                    int cooldown = 0;
                    userCooldown.TryGetValue(Context.User.Id, out cooldown);
                    if (Environment.TickCount >= cooldown)
                    {
                        cooldown = Environment.TickCount + 30000;
                        userCooldown.TryUpdate(Context.User.Id, cooldown);
                    }
                    else
                    {
                        float time = (cooldown - Environment.TickCount) / 1000;
                        int remainingTime = (int) Math.Round(time);
                        await Context.Channel.SendMessageAsync(
                            $":no_entry_sign: You are still on cooldown! Wait another {remainingTime} seconds!");
                        return;
                    }
                }
                else
                {
                    var cooldown = Environment.TickCount + 30000;
                    userCooldown.TryAdd(Context.User.Id, cooldown);
                }
                if (userBG.ContainsKey(user.Id))
                {
                    await DrawText(user.GetAvatarUrl(), user, Context);
                }
                else
                {
                    await DrawText2(user.GetAvatarUrl(), user, Context);
                }
                //await Context.Channel.SendMessageAsync($"Image \n{img}");
                if (File.Exists($"{user.Username}.png"))
                {
                    await Context.Channel.SendFileAsync($"{user.Username}.png", null, false, null);
                    File.Delete($"{user.Username}.png");
                    File.Delete($"{user.Username}Avatar.png");
                    File.Delete($"{user.Username}AvatarF.png");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Failed to create Image");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        private async Task DrawText(String AvatarUrl, IUser userInfo, CommandContext Context)
        {
            try
            {
                var fontFamily = new FontFamily("lato");
                System.Drawing.Image img = new Bitmap(900, 500);

                Graphics drawing = Graphics.FromImage(img);

                System.Drawing.Color backColor = Color.Gainsboro;

                var bgIMG = System.Drawing.Image.FromFile($"{userInfo.Username}#{userInfo.Discriminator}BGF.jpg");
                var statMask = System.Drawing.Image.FromFile($"moreBGtemp.png");

                Point point = new Point(0, 0);

                drawing.DrawImage(bgIMG, point);
                drawing.DrawImage(statMask, point);
                bgIMG.Dispose();
                statMask.Dispose();

                if (String.IsNullOrEmpty(AvatarUrl))
                    AvatarUrl =
                        "http://is2.mzstatic.com/image/pf/us/r30/Purple7/v4/89/51/05/89510540-66df-9f6f-5c91-afa5e48af4e8/mzl.sbwqpbfh.png";

                Uri requestUri = new Uri(AvatarUrl);

                if (File.Exists($"{userInfo.Username}Avatar.png"))
                {
                    File.Delete($"{userInfo.Username}Avatar.png");
                }

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                using (
                    Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream($"{userInfo.Username}Avatar.png", FileMode.Create, FileAccess.Write,
                            FileShare.None, 3145728, true))
                {
                    await contentStream.CopyToAsync(stream);
                    await contentStream.FlushAsync();
                    contentStream.Dispose();
                    await stream.FlushAsync();
                    stream.Dispose();
                    Console.WriteLine("DONE STREAM");
                }


                /*
                Image img;
                using (var bmpTemp = new Bitmap("image_file_path"))
                {
                    img = new Bitmap(bmpTemp);
                }*/

                var pointA = new Point(profileX1, profileY1);
                //var resizedImg = ResizeImage(avatarIMG, 57, 57);

                //IMAGE RESIZE
                int size = profileSIZE1;

                Configuration.Default.AddImageFormat(new PngFormat());

                using (var input = File.OpenRead($"{userInfo.Username}Avatar.png"))
                {
                    using (var output = File.OpenWrite($"{userInfo.Username}AvatarF.png"))
                    {
                        var image = new ImageSharp.Image(input)
                            .Resize(new ResizeOptions
                            {
                                Size = new ImageSharp.Size(size, size),
                                Mode = ResizeMode.Max
                            });
                        //image.ExifProfile = null; TODO FIX THIS
                        //image.Quality = quality;
                        image.Save(output);
                        image.Dispose();
                        await output.FlushAsync();
                        output.Dispose();
                        await input.FlushAsync();
                        input.Dispose();
                    }
                }
                //IMAGE RESIZE END

                var avatarIMG = System.Drawing.Image.FromFile($"{userInfo.Username}AvatarF.png");
                if (avatarIMG == null)
                {
                    Console.WriteLine("COULDNT DOWNLOAD IMAGE. AVATAR NULL");
                    return;
                }

                drawing.DrawImage(avatarIMG, pointA);

                Font font = new Font(fontFamily, 54.0F, FontStyle.Bold);
                System.Drawing.Color textColor = Color.White;
                Brush textBrush = new SolidBrush(textColor);
                System.Drawing.Color epColor = Color.Gray;
                Brush epBrush = new SolidBrush(epColor);

                drawing.DrawString($"{userInfo.Username}", font, textBrush, 288, 300);

                var fontEP = new Font(fontFamily, 36F, FontStyle.Bold);
                userStruct user = new userStruct();
                if (userEPDict.ContainsKey(userInfo.Id))
                {
                    userEPDict.TryGetValue(userInfo.Id, out user);
                    drawing.DrawString($"Rank: 10", fontEP, epBrush, 230, 420);
                    drawing.DrawString($"Level: {user.level}", fontEP, epBrush, 440, 420);
                    drawing.DrawString($"EP: {user.ep}", fontEP, epBrush, 620, 420);
                    
                }
                else
                {
                    drawing.DrawString($"EP: 0", fontEP, epBrush, 80, 70);
                    drawing.DrawString($"Level: 0", fontEP, epBrush, 170, 70);
                }
                //level = constant * sqrt(XP)

                //DONE DRAWING
                drawing.Save();

                textBrush.Dispose();
                drawing.Dispose();

                var myEncoderParameters = new EncoderParameters(1);
                var myEncoder = System.Drawing.Imaging.Encoder.Quality;
                var myImageCodecInfo = GetEncoderInfo("image/jpeg");
                // Save the bitmap as a JPEG file with quality level 25.
                var myEncoderParameter = new EncoderParameter(myEncoder, 75L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                //img.Save("test.jpg", myImageCodecInfo, myEncoderParameter);
                if (File.Exists($"{userInfo.Username}.png"))
                {
                    File.Delete($"{userInfo.Username}.png");
                }
                img.Save($"{userInfo.Username}.png");

                //Dispose avatar so it can be deleted
                avatarIMG.Dispose();

                img.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        private async Task DrawText2(String AvatarUrl, IUser userInfo, CommandContext Context)
        {
            try
            {
                var fontFamily = new FontFamily("lato");
                System.Drawing.Image img = new Bitmap(1000, 150);

                Graphics drawing = Graphics.FromImage(img);

                System.Drawing.Color backColor = Color.Gainsboro;
                var bgIMG = System.Drawing.Image.FromFile($"profilecardtemplate.png");
                var mask = System.Drawing.Image.FromFile($"ProfileMASK.png");

                Point point = new Point(0, 0);

                drawing.DrawImage(bgIMG, point);
                bgIMG.Dispose();
                

                if (String.IsNullOrEmpty(AvatarUrl))
                    AvatarUrl =
                        "http://is2.mzstatic.com/image/pf/us/r30/Purple7/v4/89/51/05/89510540-66df-9f6f-5c91-afa5e48af4e8/mzl.sbwqpbfh.png";

                Uri requestUri = new Uri(AvatarUrl);

                if (File.Exists($"{userInfo.Username}Avatar.png"))
                {
                    File.Delete($"{userInfo.Username}Avatar.png");
                }

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                using (
                    Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream($"{userInfo.Username}Avatar.png", FileMode.Create, FileAccess.Write,
                            FileShare.None, 3145728, true))
                {
                    await contentStream.CopyToAsync(stream);
                    await contentStream.FlushAsync();
                    contentStream.Dispose();
                    await stream.FlushAsync();
                    stream.Dispose();
                    Console.WriteLine("DONE STREAM");
                }


                /*
                Image img;
                using (var bmpTemp = new Bitmap("image_file_path"))
                {
                    img = new Bitmap(bmpTemp);
                }*/

                var pointA = new Point(profileX, profileY);
                //var resizedImg = ResizeImage(avatarIMG, 57, 57);

                //IMAGE RESIZE
                int size = profileSIZE;

                Configuration.Default.AddImageFormat(new PngFormat());

                using (var input = File.OpenRead($"{userInfo.Username}Avatar.png"))
                {
                    using (var output = File.OpenWrite($"{userInfo.Username}AvatarF.png"))
                    {
                        var image = new ImageSharp.Image(input)
                            .Resize(new ResizeOptions
                            {
                                Size = new ImageSharp.Size(size, size),
                                Mode = ResizeMode.Max
                            });
                        //image.ExifProfile = null; TODO FIX THIS
                        //image.Quality = quality;
                        image.Save(output);
                        image.Dispose();
                        await output.FlushAsync();
                        output.Dispose();
                        await input.FlushAsync();
                        input.Dispose();
                    }
                }
                //IMAGE RESIZE END

                var avatarIMG = System.Drawing.Image.FromFile($"{userInfo.Username}AvatarF.png");
                if (avatarIMG == null)
                {
                    Console.WriteLine("COULDNT DOWNLOAD IMAGE. AVATAR NULL");
                    return;
                }

                drawing.DrawImage(avatarIMG, pointA);
                drawing.DrawImage(mask, point);
                mask.Dispose();
                Font font = new Font(fontFamily, 45.0F, FontStyle.Bold);
                System.Drawing.Color textColor = Color.FromArgb(35, 152, 225);
                Brush textBrush = new SolidBrush(textColor);
                System.Drawing.Color epColor = Color.Black;
                Brush epBrush = new SolidBrush(epColor);

                //GET RANK

                var guild = ((SocketGuild) Context.Guild);
                //guild.DownloadUsersAsync();

                if (guild.MemberCount < 200)
                {
                    guild.DownloadUsersAsync().Wait();
                    //await guild.DownloadUsersAsync();
                }

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
                }

                //GETLIST
                var sortedList = epList.OrderByDescending(pair => pair.Value)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                var rank = GetIndex(sortedList, $"{userInfo.Username}#{userInfo.Discriminator}") + 1;
                //END RANK

                drawing.DrawString($"{userInfo.Username}", font, textBrush, 200, 10);

                var fontEP = new Font(fontFamily, 30F, FontStyle.Bold);
                userStruct user = new userStruct();
                if (userEPDict.ContainsKey(userInfo.Id))
                {
                    userEPDict.TryGetValue(userInfo.Id, out user);
                    drawing.DrawString($"EP: {user.ep}", fontEP, epBrush, 200, 80);
                    drawing.DrawString($"Level: {user.level}", fontEP, epBrush, 450, 80);
                    drawing.DrawString($"Rank: {rank}", fontEP, epBrush, 700, 80);
                }
                else
                {
                    drawing.DrawString($"EP: 0", fontEP, epBrush, 200, 80);
                    drawing.DrawString($"Level: 0", fontEP, epBrush, 450, 80);
                    drawing.DrawString($"Rank: 0", fontEP, epBrush, 700, 80);
                }
                //level = constant * sqrt(XP)

                //DONE DRAWING
                drawing.Save();

                textBrush.Dispose();
                drawing.Dispose();

                var myEncoderParameters = new EncoderParameters(1);
                var myEncoder = System.Drawing.Imaging.Encoder.Quality;
                var myImageCodecInfo = GetEncoderInfo("image/jpeg");
                // Save the bitmap as a JPEG file with quality level 25.
                var myEncoderParameter = new EncoderParameter(myEncoder, 75L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                //img.Save("test.jpg", myImageCodecInfo, myEncoderParameter);
                if (File.Exists($"{userInfo.Username}.png"))
                {
                    File.Delete($"{userInfo.Username}.png");
                }
                img.Save($"{userInfo.Username}.png");

                //Dispose avatar so it can be deleted
                avatarIMG.Dispose();

                img.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        public async Task ToggleEPSubscribe(CommandContext context)
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


        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            if (image == null)
            {
                Console.WriteLine("IMAGE NULL U FUCK");
                return null;
            }
            var destRect = new Rectangle(0, 0, width, height);
            Console.WriteLine($"HORIZONTAL: {image.HorizontalResolution}, VERTICAL: {image.VerticalResolution}");
            var destImage = new Bitmap((int) image.HorizontalResolution, (int) image.VerticalResolution);


            //destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            if (destImage == null)
            {
                Console.WriteLine("DESTIMAGE NULL");
                return null;
            }
            return destImage;
        }

        public async Task IncreaseEP(SocketMessage msg)
        {
            //Don't process the comand if it was a system message
            var message = msg as SocketUserMessage;
            if (message == null) return;


            //Create a command Context
            var context = new CommandContext(client, message);
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
                    int previousLvl = user.level;
                    user.ep += CalculateEP(context);
                    user.level = (int) Math.Round(0.15F * Math.Sqrt(user.ep));
                    if (previousLvl != user.level)
                    {
                        if (lvlSubsriberList.Contains(context.User.Id))
                        {
                            await (await context.User.CreateDMChannelAsync()).SendMessageAsync(
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

        private float CalculateEP(CommandContext context)
        {
            int lenght = (int) Math.Round(context.Message.Content.Length / 10F);
            return lenght;
        }

        private struct userStruct
        {
            public float ep;
            public int level;
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
                        userEPDict = jSerializer.Deserialize<ConcurrentDictionary<ulong, userStruct>>(reader);
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
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using System.Runtime.InteropServices;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Core;
using Sora_Bot_1.SoraBot.Services;

namespace Sora_Bot_1.SoraBot.Modules.InfoModule
{
    //create a module with the 'sample' prefix
    [Group("info")]
    public class InfoModule : ModuleBase
    {
        /*
        //$sample square 20 -> 400
        [Command("square"), Summary("Squares a number.")]
        public async Task Square([Summary("The number to square.")]int num)
        {
            //We can also access the channel from the command context
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }
        */

        private MusicService musicService;

        public InfoModule(MusicService _service)
        {
            musicService = _service;
        }

        private Process ps()
        {
            var ps = new ProcessStartInfo
            {
                FileName = "ps",
                Arguments =
                    $"auwx {Process.GetCurrentProcess().Id}"
            };
            return Process.Start(ps);
        }

        [Command(""), Summary("Gives infos about the bot")]
        public async Task BotInfo()
        {

            var proc = System.Diagnostics.Process.GetCurrentProcess();

            DiscordSocketClient _client = Context.Client as DiscordSocketClient;
            Func<double, double> formatRamValue = d =>
            {
                while (d > 1024)
                    d /= 1024;

                return d;
            };

            Func<long, string> formatRamUnit = d =>
            {
                var units = new string[] { "B", "kB", "mB", "gB" };
                var unitCount = 0;
                while (d > 1024)
                {
                    d /= 1024;
                    unitCount++;
                }

                return units[unitCount];
            };

            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.GetAvatarUrl()
                }
            };
            
            eb.AddField((efb) =>
            {
                efb.Name = "System";
                efb.IsInline = true;
                efb.Value =
                        $"**OS version:**\t{(RuntimeInformation.OSDescription.Length >= 37 ? RuntimeInformation.OSDescription.Remove(37) : RuntimeInformation.OSDescription)}\n**Architecture:**\t{RuntimeInformation.OSArchitecture}\n**Framework:**\t{RuntimeInformation.FrameworkDescription}";
            });

            eb.AddField((efb) =>
            {
                efb.Name = "Sora";
                efb.IsInline = true;
                efb.Value = $"**Up time:**\t{(DateTime.Now - proc.StartTime).ToString(@"d'd 'hh\:mm\:ss")}\n**Memory:**\t{formatRamValue(proc.PagedMemorySize64).ToString("f2")} {formatRamUnit(proc.PagedMemorySize64)}\n**Processor time:**\t{proc.TotalProcessorTime.ToString(@"d'd 'hh\:mm\:ss")}\n**Feedback or Suggestions here:**\n[Click to Join](https://discord.gg/Pah4yj5)";
            });

            eb.AddField((efb) =>
            {
                efb.Name = "Discord";
                efb.IsInline = true;

                var channelCount = 0;
                var userCount = 0;
                foreach (var g in _client.Guilds)
                {
                    channelCount += g.Channels.Count;
                    userCount += g.MemberCount;
                }

                efb.Value = $"**State:**\t{_client.ConnectionState}\n**Guilds:**\t{_client.Guilds.Count}\n**Channels:**\t{channelCount}\n**Users:**\t{userCount}\n**Playing music for:** \t{musicService.PlayingFor()} guilds\n**Ping:**\t{_client.Latency} ms";
            });
            
            await Context.Channel.SendMessageAsync("", false, eb);
        }

        // $sample userinfo --> foxbot#0282
        // $sample userinfo @Khionu --> Khionu#8708
        // $sample userinfo Khionu#8708 --> Khionu#8708
        // $sample userinfo Khionu --> Khionu#8708
        // $sample userinfo 96642168176807936 --> Khionu#8708
        // $sample whois 96642168176807936 --> Khionu#8708
        [Command("user"), Summary("Returns info about the current user, or the user paramter, if one passed.")]
        [Alias("userinfo", "whois")]
        public async Task UserInfo([Summary("The (optional) user to get info for")] IUser user = null)
        {
            try
            {
                var userInfo = user ?? Context.User; // ?? if not null return left. if null return right
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    ThumbnailUrl = userInfo.GetAvatarUrl(),
                    Title = $"{userInfo.Username}#{userInfo.Discriminator} Info",
                    Description = $"Joined Discord on: {userInfo.CreatedAt.ToString().Remove(userInfo.CreatedAt.ToString().Length - 6)}",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | {userInfo.Username} ID: {userInfo.Id}",
                        IconUrl = Context.User.GetAvatarUrl()
                    }
                };

                eb.AddField((x) =>
                {
                    x.Name = "Status";
                    x.IsInline = true;
                    x.Value = userInfo.Status.ToString();
                });

                eb.AddField((x) =>
                {
                    x.Name = "Game";
                    x.IsInline = true;
                    x.Value = $"{(userInfo.Game.HasValue ? userInfo.Game.Value.Name : "none")}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Avatar";
                    x.IsInline = true;
                    x.Value = $"[Click to View]({userInfo.GetAvatarUrl()})";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Joined Guild";
                    x.IsInline = true;
                    x.Value = $"{(userInfo as SocketGuildUser)?.JoinedAt.ToString().Remove((userInfo as SocketGuildUser).JoinedAt.ToString().Length - 6)}";
                });

                string permissions = "";
                (userInfo as SocketGuildUser)?.GuildPermissions.ToList().ForEach(x => { permissions += x.ToString() + " | "; });
                eb.AddField((x) =>
                {
                    x.Name = "Guild Permissions";
                    x.IsInline = true;
                    x.Value = $"{permissions}";
                });
                /*
                eb.AddField((efb) =>
                {
                    efb.Name = "User Info";
                    efb.IsInline = true;
                    efb.Value = $"**Name + Discriminator:** \t{userInfo.Username}#{userInfo.Discriminator} \n" +
                                $"**ID** \t{userInfo.Id}\n" +
                                $"**Created at:** \t{userInfo.CreatedAt.ToString().Remove(userInfo.CreatedAt.ToString().Length -6)} \n" +
                                $"**Status:** \t{userInfo.Status}\n" +
                                $"**Avatar:** \t[Link]({userInfo.AvatarUrl})";
                });*/

                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
            
        }

        [Command("guild", RunMode = RunMode.Async), Summary("Returns info about the current Guild")]
        [Alias("server", "serverinfo")]
        public async Task ServerInfo()
        {
            try
            {
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Title = $"{Context.Guild.Name} info",
                    Description = $"Created on {Context.Guild.CreatedAt.ToString().Remove(Context.Guild.CreatedAt.ToString().Length - 6)}",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | Guild ID: {Context.Guild.Id}",
                        IconUrl = Context.User.GetAvatarUrl()
                    }
                };
                var guild = ((SocketGuild)Context.Guild);
                //await guild.DownloadUsersAsync();

                //var onlineCount = users.Count(u => u.Status != UserStatus.Unknown && u.Status != UserStatus.Invisible && u.Status != UserStatus.Offline);
                
                var GuildOwner = await Context.Guild.GetUserAsync(Context.Guild.OwnerId);
                int online = 0;
                foreach (var u in guild.Users)
                {
                    if (u.Status != UserStatus.Unknown && u.Status != UserStatus.Invisible && u.Status != UserStatus.Offline)
                    {
                        online++;
                    }
                }
                /*
                 * 
                 * int online2 = 0;
                int realMemebers = 0;
                foreach (var u in guild.Users)
                {
                    if (u.Status != UserStatus.Unknown && u.Status != UserStatus.Invisible && u.Status != UserStatus.Offline && !u.IsBot)
                    {
                        online2++;
                    }
                    if (!u.IsBot)
                        realMemebers++;
                }
                 * 
                 * 
                eb.AddField((efb) =>
                {
                    efb.Name = "Guild";
                    efb.IsInline = true;
                    efb.Value = $"**Name:** \t{Context.Guild.Name} \n" + $"**ID:** \t{Context.Guild.Id}\n" + $"**Owner:** \t{GuildOwner}\n" +
                                $"**Voice Region ID:** \t{Context.Guild.VoiceRegionId}\n" +
                                $"**Created At:** \t{Context.Guild.CreatedAt.ToString().Remove(Context.Guild.CreatedAt.ToString().Length - 6)}";
                });*/

                eb.AddField((x) =>
                {
                    x.Name = "Owner";
                    x.IsInline = true;
                    x.Value = GuildOwner.Username;
                });

                eb.AddField((x) =>
                {
                    x.Name = "Region";
                    x.IsInline = true;
                    x.Value = Context.Guild.VoiceRegionId;
                });

                eb.AddField((x) =>
                {
                    x.Name = "Roles";
                    x.IsInline = true;
                    x.Value = "" + Context.Guild.Roles.Count;
                });

                int voice = Context.Guild.GetVoiceChannelsAsync().Result.Count;
                int text = Context.Guild.GetChannelsAsync().Result.Count - voice;
                eb.AddField((x) =>
                {
                    x.Name = "Channels";
                    x.IsInline = true;
                    x.Value = $"{text} text, {voice} voice";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Total Members";
                    x.IsInline = true;
                    x.Value = $"{online}/{((SocketGuild)Context.Guild).MemberCount}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Avatar Url";
                    x.IsInline = true;
                    x.Value = $"[Click to view]({Context.Guild.IconUrl})";
                });

                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
            
        }
    }
}

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using System.Runtime.InteropServices;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Core;
using Sora_Bot_1.SoraBot.Services;
using System.IO;
using System.Collections;

namespace Sora_Bot_1.SoraBot.Modules.InfoModule
{

    [Group("info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private MusicService musicService;
        private CommandHandler _commandHandler;

        public InfoModule(MusicService _service, CommandHandler handler)
        {
            musicService = _service;
            _commandHandler = handler;
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
            double VSZ = 0;
            double RSS = 0;
            try
            {
                if (File.Exists($"/proc/{proc.Id}/statm"))
                {
                    var ramusageInitial = File.ReadAllText($"/proc/{proc.Id}/statm");
                    var ramusage = ramusageInitial.Split(' ')[0];
                    VSZ = double.Parse(ramusage);
                    VSZ = VSZ * 4096 / 1048576;
                    ramusage = ramusageInitial.Split(' ')[1];
                    RSS = double.Parse(ramusage);
                    RSS = RSS * 4096 / 1048576;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }

            var ebn = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Title = "**Sora Info**",
                Url = "http://git.argus.moe/serenity/SoraBot"
            };
            ebn.AddField((x) =>
            {
                x.Name = "Uptime";
                x.IsInline = true;
                x.Value = (DateTime.Now - proc.StartTime).ToString(@"d'd 'hh\:mm\:ss");
            });

            ebn.AddField((x) =>
            {
                x.Name = ".NET Framework";
                x.IsInline = true;
                x.Value = RuntimeInformation.FrameworkDescription;
            });

            ebn.AddField((x) =>
            {
                x.Name = "Used RAM";
                x.IsInline = true;
                x.Value = $"{(proc.PagedMemorySize64 == 0 ? $"{RSS.ToString("f1")} mB / {VSZ.ToString("f1")} mB" : $"{formatRamValue(proc.PagedMemorySize64).ToString("f2")} {formatRamUnit(proc.PagedMemorySize64)} / {formatRamValue(proc.VirtualMemorySize64).ToString("f2")} {formatRamUnit(proc.VirtualMemorySize64)}")}";
            });
            ebn.AddField((x) =>
            {
                x.Name = "Commands Executed";
                x.IsInline = true;
                x.Value = $"{_commandHandler.CommandsRunSinceRestart()} since restart";
            });
            ebn.AddField((x) =>
            {
                x.Name = "Threads running";
                x.IsInline = true;
                x.Value = $"{((IEnumerable)proc.Threads).OfType<ProcessThread>().Where(t=>t.ThreadState == ThreadState.Running).Count()} / {proc.Threads.Count}";
            });
            ebn.AddField((x) =>
            {
                x.Name = "Connected Guilds";
                x.IsInline = true;
                x.Value = $"{_client.Guilds.Count}";
            });
            var channelCount = 0;
            var userCount = 0;
            foreach (var g in _client.Guilds)
            {
                channelCount += g.Channels.Count;
                userCount += g.MemberCount;
            }
            ebn.AddField((x) =>
            {
                x.Name = "Watching Channels";
                x.IsInline = true;
                x.Value = $"{channelCount}";
            });
            ebn.AddField((x) =>
            {
                x.Name = "Users with access";
                x.IsInline = true;
                x.Value = $"{userCount}";
            });
            ebn.AddField((x) =>
            {
                x.Name = "Playing music for";
                x.IsInline = true;
                x.Value = $"{musicService.PlayingFor()} guilds";
            });
            ebn.AddField((x) =>
            {
                x.Name = "Ping";
                x.IsInline = true;
                x.Value = $"{_client.Latency} ms";
            });
            ebn.AddField((x) =>
            {
                x.Name = "Sora's Official Guild";
                x.IsInline = true;
                x.Value = $"[Feedback and Suggestions here](https://discord.gg/Pah4yj5)";
            });
            

            await Context.Channel.SendMessageAsync("", false, ebn);
            /*
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
                efb.Value = $"**Up time:**\t{(DateTime.Now - proc.StartTime).ToString(@"d'd 'hh\:mm\:ss")}\n" +
                $"{(proc.PagedMemorySize64 == 0 ? $"**Memory:**\t{RSS.ToString("f1")} mB / {VSZ.ToString("f1")} mB" : $"**Memory:**\t{formatRamValue(proc.PagedMemorySize64).ToString("f2")} {formatRamUnit(proc.PagedMemorySize64)}")}" +
                $"{(Context.User.Id == 192750776005689344 ? $"\n**PROCESS ID**:\t{proc.Id}" : "")}\n**Processor time:**\t{proc.TotalProcessorTime.ToString(@"d'd 'hh\:mm\:ss")}\n**Feedback or Suggestions here:**\n[Click to Join](https://discord.gg/Pah4yj5)";
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
            });*/
        }

        [Command("permissions"), Summary("Returns the guild perms of the current user, or the user paramter, if one passed.")]
        [Alias("perms")]
        public async Task UserPerms([Summary("The (optional) user to get guild perms for")] IUser user = null)
        {
            var userInfo = user ?? Context.User; // ?? if not null return left. if null return right
            string permissions = "";
            //GuildPermissions.Has(GuildPermission.BanMembers)
            var invoker = Context.User as IGuildUser;
            if(!invoker.GuildPermissions.Has(GuildPermission.BanMembers) && !invoker.GuildPermissions.Has(GuildPermission.KickMembers) && !invoker.GuildPermissions.Has(GuildPermission.Administrator) && !invoker.GuildPermissions.Has(GuildPermission.ManageChannels) && !invoker.GuildPermissions.Has(GuildPermission.ManageGuild))
            {
                await ReplyAsync(":no_entry_sign: You need at least some sort of Guild Permission. Admin, Kick, Ban, Manage Channels, Manage Guild. Something...");
                return;
            }
            (userInfo as SocketGuildUser)?.GuildPermissions.ToList().ForEach(x => { permissions += x.ToString() + " , "; });

            var avatarURL = userInfo.GetAvatarUrl() ??
                                "http://ravegames.net/ow_userfiles/themes/theme_image_22.jpg";

            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247),
                ThumbnailUrl = avatarURL,
                Title = $"{userInfo.Username}#{userInfo.Discriminator} Guild Permissions",
                Description = $"{(String.IsNullOrWhiteSpace(permissions) ? "*none*" : permissions)}",
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.GetAvatarUrl()
                }
            };
            await ReplyAsync("", embed: eb);
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
                var avatarURL = userInfo.GetAvatarUrl() ??
                                "http://ravegames.net/ow_userfiles/themes/theme_image_22.jpg";
                   // if (String.IsNullOrEmpty(AvatarUrl))
                    //AvatarUrl = "http://is2.mzstatic.com/image/pf/us/r30/Purple7/v4/89/51/05/89510540-66df-9f6f-5c91-afa5e48af4e8/mzl.sbwqpbfh.png";
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    ThumbnailUrl = avatarURL,
                    Title = $"{userInfo.Username}",
                    Description = $"Joined Discord on {userInfo.CreatedAt.ToString().Remove(userInfo.CreatedAt.ToString().Length - 6)}. That is {(int)(DateTime.Now.Subtract(userInfo.CreatedAt.DateTime).TotalDays)} days ago!", //{(int)(DateTime.Now.Subtract(Context.Guild.CreatedAt.DateTime).TotalDays)}
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | {userInfo.Username} ID: {userInfo.Id}",
                        IconUrl = Context.User.GetAvatarUrl()
                    }
                };
                var socketUser = userInfo as SocketGuildUser;
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
                    x.Value = $"{(userInfo.Game.HasValue ? userInfo.Game.Value.Name : "*none*")}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Nickname";
                    x.IsInline = true;
                    x.Value = $"{(socketUser.Nickname == null ? "*none*": $"{socketUser.Nickname}")}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Discriminator";
                    x.IsInline = true;
                    x.Value = $"#{socketUser.Discriminator}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Avatar";
                    x.IsInline = true;
                    x.Value = $"[Click to View]({avatarURL})";
                });
                
                eb.AddField((x) =>
                {
                    x.Name = "Joined Guild";
                    x.IsInline = true;
                    x.Value = $"{socketUser?.JoinedAt.ToString().Remove(socketUser.JoinedAt.ToString().Length - 6)}\n({(int)DateTime.Now.Subtract(((DateTimeOffset)socketUser?.JoinedAt).DateTime).TotalDays} days ago)";
                });
                /*
                string permissions = "";
                (userInfo as SocketGuildUser)?.GuildPermissions.ToList().ForEach(x => { permissions += x.ToString() + " | "; });
                eb.AddField((x) =>
                {
                    x.Name = "Guild Permissions";
                    x.IsInline = true;
                    x.Value = $"{permissions}";
                });*/
                eb.AddField((x) =>
                {
                    string roles = "";
                    foreach (var role in socketUser.Roles)
                    {
                        if(role.Name != "@everyone")
                            roles += $"{role.Name}, ";
                    }
                    x.Name = "Roles";
                    x.IsInline = true;
                    x.Value = $"{(String.IsNullOrWhiteSpace(roles) ? "*none*": $"{roles}")}";
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
                var avatarURL = Context.Guild.IconUrl ??
                                "http://ravegames.net/ow_userfiles/themes/theme_image_22.jpg";
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    ThumbnailUrl = avatarURL,
                    Title = $"{Context.Guild.Name} info",
                    Description = $"Created on {Context.Guild.CreatedAt.ToString().Remove(Context.Guild.CreatedAt.ToString().Length - 6)}. That's {(int)(DateTime.Now.Subtract(Context.Guild.CreatedAt.DateTime).TotalDays)} days ago!",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | Guild ID: {Context.Guild.Id}",
                        IconUrl = Context.User.GetAvatarUrl()
                    }
                };
                var guild = ((SocketGuild)Context.Guild);
                //await guild.DownloadUsersAsync();

                //var onlineCount = users.Count(u => u.Status != UserStatus.Unknown && u.Status != UserStatus.Invisible && u.Status != UserStatus.Offline);
                
                var GuildOwner = Context.Guild.GetUser(Context.Guild.OwnerId);
                int online = 0;
                foreach (var u in guild.Users)
                {
                    if (u.Status != UserStatus.Invisible && u.Status != UserStatus.Offline)
                    {
                        online++;
                    }
                }

                eb.AddField((x) =>
                {
                    x.Name = "Owner";
                    x.IsInline = true;
                    x.Value = GuildOwner.Username;
                });

                eb.AddField((x) =>
                {
                    x.Name = "Members";
                    x.IsInline = true;
                    x.Value = $"{online} / {(Context.Guild).MemberCount}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Region";
                    x.IsInline = true;
                    x.Value = Context.Guild.VoiceRegionId.ToUpper();
                });

                eb.AddField((x) =>
                {
                    x.Name = "Roles";
                    x.IsInline = true;
                    x.Value = "" + Context.Guild.Roles.Count;
                });

                int voice = Context.Guild.VoiceChannels.Count;
                int text = Context.Guild.TextChannels.Count;
                eb.AddField((x) =>
                {
                    x.Name = "Channels";
                    x.IsInline = true;
                    x.Value = $"{text} text, {voice} voice";
                });

                eb.AddField((x) =>
                {
                    x.Name = "AFK Channel";
                    x.IsInline = true;
                    x.Value = $"{(Context.Guild.AFKChannel == null ? $"No AFK Channel" : $"{Context.Guild.AFKChannel.Name}\n*in {Context.Guild.AFKTimeout} Min*")}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Total Emojis";
                    x.IsInline = true;
                    x.Value = $"{Context.Guild.Emojis.Count}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Avatar Url";
                    x.IsInline = true;
                    x.Value = $"[Click to view]({avatarURL})";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Emojis";
                    x.IsInline = false;
                    string val = "";
                    foreach (var e in Context.Guild.Emojis)
                    {
                        if (val.Length < 950)
                            val += $"<:{e.Name}:{e.Id}> ";
                    }
                    //x.Value = $"{String.Format(":{0}:",String.Join(": , :", Context.Guild.Emojis))}";//await Context.Channel.SendMessageAsync(String.Format("Videos: \n{0}\n", String.Join("\n", videos)));
                    x.Value = val;
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

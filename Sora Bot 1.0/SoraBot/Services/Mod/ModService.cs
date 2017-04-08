using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Services.Mod
{
    public class ModService
    {
        private ConcurrentDictionary<ulong, punishStruct> _punishLogs = new ConcurrentDictionary<ulong, punishStruct>();
        private ConcurrentDictionary<ulong, modLogs> _modlogsDict = new ConcurrentDictionary<ulong, modLogs>();

        public enum Action
        {
            Ban, Kick
        }

        public ModService()
        {
            ModServiceDB.InitializeLoader();
            var modlogsTemp = ModServiceDB.LoadModLogs();
            if (modlogsTemp != null)
                _modlogsDict = modlogsTemp;
            var punishLogsTemp = ModServiceDB.LoadPunishLogs();
            if (punishLogsTemp != null)
                _punishLogs = punishLogsTemp;
        }

        public async Task AddReason(CommandContext Context, string reason)
        {
            try
            {
                if (!_punishLogs.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: No PunishLogs Channel set!");
                    return;
                }
                var mod = Context.User as SocketGuildUser;
                if (!mod.GuildPermissions.Has(GuildPermission.BanMembers) && !mod.GuildPermissions.Has(GuildPermission.KickMembers))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You don't have ban / kick permissions :frowning:");
                    return;
                }
                char[] splitter = new char[]
                {
                ' '
                };
                string[] res = reason.Split(splitter, count: 2);
                int casenr;
                if (!Int32.TryParse(res[0], out casenr))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Case number has too be... a number (Whole number / Integer)");
                    return;
                }
                punishStruct str = new punishStruct();
                _punishLogs.TryGetValue(Context.Guild.Id, out str);
                if (str.punishes == null)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: There are no logs found.");
                    return;
                }
                var channel = await Context.Guild.GetChannelAsync(str.channelID) as IMessageChannel;
                if (channel == null)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: _punishLogs Channel is not set!");
                    return;
                }
                var found = str.punishes.Where(x => x.caseNr == casenr).FirstOrDefault();
                if (found == null)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find Case!");
                    return;
                }

                var msgToEdit = (IUserMessage)await channel.GetMessageAsync(found.punishMsgID, CacheMode.AllowDownload); //(IUserMessage)
                if (msgToEdit == null)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find Punishlog!");
                    return;
                }
                if(found.modID != mod.Id)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Only the person that banned the User can edit the reason!");
                    return;
                }

                await msgToEdit.ModifyAsync((y) =>
                {
                    var ebT = new EmbedBuilder()
                    {
                        Color = new Color(4, 97, 247),
                        Title = $"Case #{found.caseNr} | {(found.type == Action.Ban ? "Ban :hammer:" : "Kick :boot:")}",
                        Timestamp = DateTimeOffset.UtcNow
                    };
                    ebT.AddField((x) =>
                    {
                        x.Name = "User";
                        x.IsInline = true;
                        x.Value = $"**{found.user}** ({found.userID})";
                    });

                    ebT.AddField((x) =>
                    {
                        x.Name = "Moderator";
                        x.IsInline = true;
                        x.Value = $"**{mod.Username}#{mod.Discriminator}** ({mod.Id})";
                    });

                    ebT.AddField((x) =>
                    {
                        x.Name = "Reason";
                        x.IsInline = true;
                        x.Value = res[1];
                    });

                    y.Embed = ebT.Build(); 
                });
                str.punishes.Remove(found);
                found.mod = $"{mod.Username}#{mod.Discriminator}";
                found.modID = mod.Id;
                found.reason = res[1];
                str.punishes.Add(found);
                _punishLogs.TryUpdate(Context.Guild.Id, str);
                var msg = await Context.Channel.SendMessageAsync(":white_check_mark: Successfully updated reason!");
                await Task.Delay(3000);
                var bot = await Context.Guild.GetUserAsync(270931284489011202, Discord.CacheMode.AllowDownload) as IGuildUser;
                if (bot.GuildPermissions.Has(GuildPermission.ManageMessages))
                {
                    await Context.Message.DeleteAsync();
                }
                await msg.DeleteAsync();
                ModServiceDB.SavePunishLogs(_punishLogs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task Kick(CommandContext Context, IUser userT, string reason)
        {
            try
            {
                var bot = await Context.Guild.GetUserAsync(270931284489011202, Discord.CacheMode.AllowDownload) as IGuildUser;
                if (!bot.GuildPermissions.Has(GuildPermission.KickMembers))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: I don't have kick permissions :frowning:");
                    return;
                }
                var mod = Context.User as SocketGuildUser;
                if (!mod.GuildPermissions.Has(GuildPermission.KickMembers))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You don't have kick permissions :frowning:");
                    return;
                }
                var modHighestRole = mod.Roles.OrderByDescending(r => r.Position).First();
                var user = userT as SocketGuildUser;
                var usersHighestRole = user.Roles.OrderByDescending(r => r.Position).First();

                if (usersHighestRole.Position > modHighestRole.Position)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You can't kick someone above you in the role hierarchy!");
                    return;
                }


                var botHighestRole = bot.RoleIds.Select(x => Context.Guild.GetRole(x))
                                               .OrderBy(x => x.Position)
                                               .First();

                if (usersHighestRole.Position > botHighestRole.Position)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: I can't kick someone above me in the role hierarchy!");
                    return;
                }

                var guild = Context.Guild as SocketGuild;
                await user.KickAsync();
                await Context.Channel.SendMessageAsync($":white_check_mark: {userT.Username}#{userT.Discriminator} has been successfully kicked :ok_hand:");
                var modT = mod as IUser;
                await LogAction(Action.Kick, userT, modT, reason, Context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Context.Channel.SendMessageAsync(":no_entry_sign: Failed to ban the user :frowning:");
                await SentryService.SendError(e, Context);
            }

        }

        public async Task PermBan(CommandContext Context, IUser userT, string reason)
        {
            try
            {
                var bot = await Context.Guild.GetUserAsync(270931284489011202, Discord.CacheMode.AllowDownload) as IGuildUser;
                if (!bot.GuildPermissions.Has(GuildPermission.BanMembers))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: I don't have ban permissions :frowning:");
                    return;
                }
                var mod = Context.User as SocketGuildUser;
                if (!mod.GuildPermissions.Has(GuildPermission.BanMembers))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You don't have ban permissions :frowning:");
                    return;
                }
                var modHighestRole = mod.Roles.OrderByDescending(r => r.Position).First();
                var user = userT as SocketGuildUser;
                var usersHighestRole = user.Roles.OrderByDescending(r => r.Position).First();

                if (usersHighestRole.Position > modHighestRole.Position)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You can't ban someone above you in the role hierarchy!");
                    return;
                }


                var botHighestRole = bot.RoleIds.Select(x => Context.Guild.GetRole(x))
                                               .OrderBy(x => x.Position)
                                               .First();

                if (usersHighestRole.Position > botHighestRole.Position)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: I can't ban someone above me in the role hierarchy!");
                    return;
                }

                var guild = Context.Guild as SocketGuild;
                await guild.AddBanAsync(userT, 2);
                await Context.Channel.SendMessageAsync($":white_check_mark: {userT.Username}#{userT.Discriminator} has been successfully banned :ok_hand:");
                var modT = mod as IUser;
                await LogAction(Action.Ban, userT, modT, reason, Context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Context.Channel.SendMessageAsync(":no_entry_sign: Failed to ban the user :frowning:");
                await SentryService.SendError(e, Context);
            }
        }

        public async Task setPunishLogsChannel(CommandContext Context, IMessageChannel channel)
        {
            try
            {
                var user = Context.User as SocketGuildUser;
                if (!user.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You need `Administrator` permissions to change the PunishLogs Channel!");
                    return;
                }
                punishStruct str = new punishStruct();
                if (!_punishLogs.ContainsKey(Context.Guild.Id))
                {
                    str.channelID = channel.Id;
                }
                else
                {
                    _punishLogs.TryGetValue(Context.Guild.Id, out str);
                    str.channelID = channel.Id;
                }
                _punishLogs.AddOrUpdate(Context.Guild.Id, str, (key, oldValue) => str);
                await Context.Channel.SendMessageAsync($":white_check_mark: Successfully added #{channel.Name} as Punishlog Channel");
                ModServiceDB.SavePunishLogs(_punishLogs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task delPunishLogsChannel(CommandContext Context)
        {
            try
            {
                var user = Context.User as SocketGuildUser;
                if (!user.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You need `Administrator` permissions to change the PunishLogs Channel!");
                    return;
                }
                if (!_punishLogs.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: No Channel has been set yet!");
                    return;
                }

                punishStruct str = new punishStruct();
                _punishLogs.TryGetValue(Context.Guild.Id, out str);
                if (str.channelID == 0)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Channel was already removed!");
                    return;
                }
                str.channelID = 0;
                _punishLogs.TryUpdate(Context.Guild.Id, str);
                await Context.Channel.SendMessageAsync(":white_check_mark: Channel has been successfully removed!");
                ModServiceDB.SavePunishLogs(_punishLogs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        private async Task LogAction(Action type, IUser user, IUser mod, string reason, CommandContext Context)
        {
            try
            {
                if (!_punishLogs.ContainsKey(Context.Guild.Id))
                    return;
                punishStruct str = new punishStruct();
                _punishLogs.TryGetValue(Context.Guild.Id, out str);
                var channel = await Context.Guild.GetChannelAsync(str.channelID) as IMessageChannel;
                if (channel == null)
                    return;
                if (str.punishes == null)
                    str.punishes = new List<punishCase>();
                var casenr = str.punishes.Count + 1;
                punishCase pnsh = new punishCase
                {
                    caseNr = casenr,
                    type = type,
                    mod = $"{mod.Username}#{mod.Discriminator}",
                    modID = mod.Id,
                    user = $"{user.Username}#{user.Discriminator}",
                    userID = user.Id,
                    reason = reason
                };

                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Title = $"Case #{pnsh.caseNr} | {(type == Action.Ban ? "Ban :hammer:" : "Kick :boot:")}",
                    Timestamp = DateTimeOffset.UtcNow
                };
                eb.AddField((x) =>
                {
                    x.Name = "User";
                    x.IsInline = true;
                    x.Value = $"**{pnsh.user}** ({pnsh.userID})";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Moderator";
                    x.IsInline = true;
                    x.Value = $"**{pnsh.mod}** ({pnsh.modID})";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Reason";
                    x.IsInline = true;
                    x.Value = $"{(String.IsNullOrWhiteSpace(pnsh.reason) ? $"Type [p]reason {pnsh.caseNr} <reason> to add it" : pnsh.reason)}";
                });

                var msg = await channel.SendMessageAsync("", embed: eb);
                pnsh.punishMsgID = msg.Id;
                str.punishes.Add(pnsh);
                _punishLogs.TryUpdate(Context.Guild.Id, str);
                ModServiceDB.SavePunishLogs(_punishLogs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        //ModLogs

        public async Task setModLgosChannel(CommandContext Context, IMessageChannel channel)
        {
            try
            {
                var user = Context.User as SocketGuildUser;
                if (!user.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You need `Administrator` permissions to change the PunishLogs Channel!");
                    return;
                }
                modLogs modLogs = new modLogs();
                if (!_modlogsDict.ContainsKey(Context.Guild.Id))
                {
                    modLogs.channelID = channel.Id;
                }
                else
                {
                    _modlogsDict.TryGetValue(Context.Guild.Id, out modLogs);
                    modLogs.channelID = channel.Id;
                }
                _modlogsDict.AddOrUpdate(Context.Guild.Id, modLogs, (key, oldValue) => modLogs);
                await Context.Channel.SendMessageAsync(":white_check_mark: Channel has been successfully added!");
                ModServiceDB.SaveModLogs(_modlogsDict);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task removeModLogsChannel(CommandContext Context)
        {
            try
            {
                var user = Context.User as SocketGuildUser;
                if (!user.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You need `Administrator` permissions to change the PunishLogs Channel!");
                    return;
                }
                if (!_modlogsDict.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: No Channel has been set yet!");
                    return;
                }
                modLogs modLogs = new modLogs();
                _modlogsDict.TryGetValue(Context.Guild.Id, out modLogs);
                if (modLogs.channelID == 0)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Channel was already removed!");
                    return;
                }
                modLogs.channelID = 0;
                _modlogsDict.TryUpdate(Context.Guild.Id, modLogs);
                await Context.Channel.SendMessageAsync(":white_check_mark: Channel has been successfully removed!");
                ModServiceDB.SaveModLogs(_modlogsDict);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task Client_MessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            try
            {
                var guild = (channel as IGuildChannel).Guild;
                if (!_modlogsDict.ContainsKey(guild.Id))
                    return;
                modLogs logs = new modLogs();
                _modlogsDict.TryGetValue(guild.Id, out logs);

                var logChannel = await guild.GetChannelAsync(logs.channelID) as IMessageChannel;
                if (logChannel == null)
                    return;
                if (!logs.msgDelete)
                    return;
                if (logs.msgDeleteInterval.CompareTo(DateTime.UtcNow) > 0)
                    return;
                logs.msgDeleteInterval = DateTime.UtcNow.AddSeconds(4);

                var eb = new EmbedBuilder() //https://img.clipartfest.com/664ce829afe3443ac3aae2f074b4bd69_recycle-bin-icon-recycle-bin-icon-clipart_2400-2400.png
                {
                    Color = new Color(4, 97, 247),
                    Title = $"{DateTime.UtcNow.TimeOfDay.ToString().Remove(8)} - Message Deleted",
                    ThumbnailUrl = "https://img.clipartfest.com/664ce829afe3443ac3aae2f074b4bd69_recycle-bin-icon-recycle-bin-icon-clipart_2400-2400.png",
                    Description = msg.Value.Content,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Author ID: {msg.Value.Author.Id}"
                    }
                };

                eb.AddField((x) =>
                {
                    x.Name = "Channel";
                    x.IsInline = true;
                    x.Value = $"#{channel.Name}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Author";
                    x.IsInline = true;
                    x.Value = $"{msg.Value.Author.Username}#{msg.Value.Author.Discriminator}";
                });

                await logChannel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }


        public async Task Client_RoleCreated(SocketRole role)
        {
            try
            {
                var guild = role.Guild as IGuild;
                if (!_modlogsDict.ContainsKey(guild.Id))
                    return;
                modLogs logs = new modLogs();
                _modlogsDict.TryGetValue(guild.Id, out logs);

                var logChannel = await guild.GetChannelAsync(logs.channelID) as IMessageChannel;
                if (logChannel == null)
                    return;
                if (!logs.roleChange)
                    return;
                if (logs.roleChangeInterval.CompareTo(DateTime.UtcNow) > 0)
                    return;
                logs.roleChangeInterval = DateTime.UtcNow.AddSeconds(10);

                var eb = createRoleEmbed(role, true);

                await logChannel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        private EmbedBuilder createRoleEmbed(SocketRole role, bool isCreate)
        {
            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247),
                Title = $"{DateTime.UtcNow.TimeOfDay.ToString().Remove(8)} - Role {(isCreate ? "Created" : "Deleted")}",
                ThumbnailUrl = "http://i.imgur.com/Qo4kSxq.png",
                Description = $"**{role.Name}** ({role.Id}) has been {(isCreate ? "created" : "deleted")}!"
            };
            eb.AddField((x) =>
            {
                x.Name = "Color";
                x.IsInline = true;
                x.Value = $"{role.Color}";
            });
            eb.AddField((x) =>
            {
                x.Name = "Is Mentionable";
                x.IsInline = true;
                x.Value = $"{role.IsMentionable}";
            });
            eb.AddField((x) =>
            {
                string msg = "";
                foreach (var p in role.Permissions.ToList())
                {
                    msg += $"{p}, ";
                }
                x.Name = "Permissions";
                x.IsInline = true;
                x.Value = $"{(String.IsNullOrWhiteSpace(msg) ? "No Permissions" : msg)}";
            });

            return eb;
        }

        public async Task Client_RoleDeleted(SocketRole role)
        {
            try
            {
                var guild = role.Guild as IGuild;
                if (!_modlogsDict.ContainsKey(guild.Id))
                    return;
                modLogs logs = new modLogs();
                _modlogsDict.TryGetValue(guild.Id, out logs);

                var logChannel = await guild.GetChannelAsync(logs.channelID) as IMessageChannel;
                if (logChannel == null)
                    return;
                if (!logs.roleChange)
                    return;
                if (logs.roleChangeInterval.CompareTo(DateTime.UtcNow) > 0)
                    return;
                logs.roleChangeInterval = DateTime.UtcNow.AddSeconds(5);

                var eb = createRoleEmbed(role, false);
                await logChannel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        public async Task Client_RoleUpdated(SocketRole oldRole, SocketRole newRole)
        {
            try
            {
                var guild = oldRole.Guild as IGuild;
                if (!_modlogsDict.ContainsKey(guild.Id))
                    return;
                modLogs logs = new modLogs();
                _modlogsDict.TryGetValue(guild.Id, out logs);

                var logChannel = await guild.GetChannelAsync(logs.channelID) as IMessageChannel;
                if (logChannel == null)
                    return;
                if (!logs.roleChange)
                    return;
                if (logs.roleChangeInterval.CompareTo(DateTime.UtcNow) > 0)
                    return;
                logs.roleChangeInterval = DateTime.UtcNow.AddSeconds(10);

                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Title = $"{DateTime.UtcNow.TimeOfDay.ToString().Remove(8)} - Role Updated",
                    ThumbnailUrl = "http://i.imgur.com/Qo4kSxq.png",
                    Description = $"**{oldRole.Name}** ({oldRole.Id}) has been updated to **{newRole.Name}** ({newRole.Id})"
                };
                eb.AddField((x) =>
                {
                    x.Name = "Color";
                    x.IsInline = true;
                    x.Value = $"{oldRole.Color} : {newRole.Color}";
                });
                eb.AddField((x) =>
                {
                    x.Name = "Is Mentionable";
                    x.IsInline = true;
                    x.Value = $"{oldRole.IsMentionable} : {newRole.IsMentionable}";
                });
                eb.AddField((x) =>
                {
                    string msg1 = "";
                    foreach (var p in oldRole.Permissions.ToList())
                    {
                        msg1 += $"{p}, ";
                    }

                    string msg2 = "";
                    foreach (var p in newRole.Permissions.ToList())
                    {
                        msg2 += $"{p}, ";
                    }
                    x.Name = "Permissions";
                    x.IsInline = true;
                    x.Value = $"{(String.IsNullOrWhiteSpace(msg1) ? "No Permissions" : msg1)} \n\n{(String.IsNullOrWhiteSpace(msg2) ? "No Permissions" : msg2)}";
                });

                await logChannel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        private EmbedBuilder _createChannelEmbed(SocketChannel channel, bool isCreate)
        {
            var ichannel = channel as IMessageChannel;
            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247),
                Title = $"{DateTime.UtcNow.TimeOfDay.ToString().Remove(8)} - Channel was {(isCreate ? "Created" : "Deleted")}",
                ThumbnailUrl = "https://cdn0.iconfinder.com/data/icons/kirrkle-social-networks-part-1/60/05_-_Text_messaging-512.png",
                Description = $"**#{ichannel.Name}** ({ichannel.Id}) has been {(isCreate ? "created" : "deleted")}!"
            };
            return eb;
        }


        public async Task Client_ChannelCreated(SocketChannel channel)
        {
            try
            {
                var guild = (channel as IGuildChannel).Guild;
                if (!_modlogsDict.ContainsKey(guild.Id))
                    return;
                modLogs logs = new modLogs();
                _modlogsDict.TryGetValue(guild.Id, out logs);

                var logChannel = await guild.GetChannelAsync(logs.channelID) as IMessageChannel;
                if (logChannel == null)
                    return;
                if (!logs.channelChange)
                    return;
                if (logs.channelChangeInterval.CompareTo(DateTime.UtcNow) > 0)
                    return;
                logs.channelChangeInterval = DateTime.UtcNow.AddSeconds(10);

                var eb = _createChannelEmbed(channel, true);

                await logChannel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        public async Task Client_ChannelUpdated(SocketChannel oldChannel, SocketChannel newChannel)
        {
            try
            {
                var socketOld = oldChannel as SocketGuildChannel;
                var socketNew = newChannel as SocketGuildChannel;
                if (socketOld.Position != socketNew.Position)
                    return;
                var guild = (oldChannel as IGuildChannel).Guild;
                if (!_modlogsDict.ContainsKey(guild.Id))
                    return;
                modLogs logs = new modLogs();
                _modlogsDict.TryGetValue(guild.Id, out logs);

                var logChannel = await guild.GetChannelAsync(logs.channelID) as IMessageChannel;
                if (logChannel == null)
                    return;
                if (!logs.channelChange)
                    return;
                if (logs.channelChangeInterval.CompareTo(DateTime.UtcNow) > 0)
                    return;
                logs.channelChangeInterval = DateTime.UtcNow.AddSeconds(10);


                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Title = $"{DateTime.UtcNow.TimeOfDay.ToString().Remove(8)} - Channel was Updated",
                    ThumbnailUrl = "https://cdn0.iconfinder.com/data/icons/kirrkle-social-networks-part-1/60/05_-_Text_messaging-512.png",
                    Description = $"**#{socketOld.Name}** ({socketOld.Id}) \nhas been changed to \n**#{socketNew.Name}** ({socketNew.Id})!"
                };
                

                await logChannel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        public async Task Client_ChannelDestroyed(SocketChannel channel)
        {
            try
            {
                var guild = (channel as IGuildChannel).Guild;
                if (!_modlogsDict.ContainsKey(guild.Id))
                    return;
                modLogs logs = new modLogs();
                _modlogsDict.TryGetValue(guild.Id, out logs);

                var logChannel = await guild.GetChannelAsync(logs.channelID) as IMessageChannel;
                if (logChannel == null)
                    return;
                if (!logs.channelChange)
                    return;
                if (logs.channelChangeInterval.CompareTo(DateTime.UtcNow) > 0)
                    return;
                logs.channelChangeInterval = DateTime.UtcNow.AddSeconds(5);

                var eb = _createChannelEmbed(channel, false);

                await logChannel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        public async Task Client_GuildUpdated(SocketGuild guildOld, SocketGuild guildNew)
        {
            if (!_modlogsDict.ContainsKey(guildOld.Id))
                return;
            modLogs logs = new modLogs();
            _modlogsDict.TryGetValue(guildOld.Id, out logs);

            var logChannel = await (guildOld as IGuild).GetChannelAsync(logs.channelID) as IMessageChannel;
            if (logChannel == null)
                return;
            if (!logs.serverChange)
                return;
            if (logs.serverChangeInterval.CompareTo(DateTime.UtcNow) > 0)
                return;
            logs.serverChangeInterval = DateTime.UtcNow.AddSeconds(5);

            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247),
                Title = $"{DateTime.UtcNow.TimeOfDay.ToString().Remove(8)} - Server was Updated",
                ThumbnailUrl = "https://cdn1.iconfinder.com/data/icons/round-ui/143/33-512.png",
                Description = $"{(guildOld.Name != guildNew.Name ? $"**{guildOld.Name}**\nhas been changed to \n**{guildNew.Name}**" : $"**{guildOld}** - Name hasn't changed")}!"
            };

            if (guildOld.IconUrl != guildNew.IconUrl)
            {
                eb.AddField((x) =>
                {
                    x.Name = "Guild Avatar Updated";
                    x.IsInline = true;
                    x.Value = $"[Old Avatar]({guildOld.IconUrl}) : [New Avatar]({guildNew.IconUrl})";
                });
            }
            if(guildOld.VoiceRegionId != guildNew.VoiceRegionId)
            {
                eb.AddField((x) =>
                {
                    x.Name = "Voice Region Updated";
                    x.IsInline = true;
                    x.Value = $"{guildOld.VoiceRegionId} : {guildNew.VoiceRegionId}";
                });
            }
            if(guildOld.VerificationLevel != guildNew.VerificationLevel)
            {
                eb.AddField((x) =>
                {
                    x.Name = "Verification Level Updated";
                    x.IsInline = true;
                    x.Value = $"{guildOld.VerificationLevel} : {guildNew.VerificationLevel}";
                });
            }
            if(guildOld.OwnerId != guildNew.OwnerId)
            {
                eb.AddField((x) =>
                {
                    x.Name = "Owner Changed";
                    x.IsInline = true;
                    x.Value = $"{guildOld.Owner.Username}#{guildOld.Owner.Discriminator} ({guildOld.OwnerId})\n{guildNew.Owner.Username}#{guildNew.Owner.Discriminator} ({guildNew.OwnerId})";
                });
            }

            await logChannel.SendMessageAsync("", embed: eb);

        }

        public async Task ShowConfigLog(CommandContext Context)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You need `Administrator` permissions to view the ModLog config!");
                return;
            }
            if (!_modlogsDict.ContainsKey(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You have not set any ModLogs yet! Your config file will be created when you create the Channel for the first time!");
            }

            modLogs modLogs = new modLogs();
            _modlogsDict.TryGetValue(Context.Guild.Id, out modLogs);

            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247),
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Title = $"ModLog Config File for {Context.Guild.Name}"
            };

            eb.AddField((x) =>
            {
                x.Name = "Channel";
                x.IsInline = true;
                x.Value = $"<#{modLogs.channelID}>";
            });

            eb.AddField((x) =>
            {
                x.Name = "Role Change";
                x.IsInline = true;
                x.Value = $"{modLogs.roleChange}";
            });

            eb.AddField((x) =>
            {
                x.Name = "Server Change";
                x.IsInline = true;
                x.Value = $"{modLogs.serverChange}";
            });

            eb.AddField((x) =>
            {
                x.Name = "Channel Change";
                x.IsInline = true;
                x.Value = $"{modLogs.channelChange}";
            });

            eb.AddField((x) =>
            {
                x.Name = "Msg Delete";
                x.IsInline = true;
                x.Value = $"{modLogs.msgDelete}";
            });

            await Context.Channel.SendMessageAsync("", embed: eb);
        }

        public async Task ToggleRole(CommandContext Context)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You need `Administrator` permissions to change the ModLog config!");
                return;
            }
            if (!_modlogsDict.ContainsKey(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You have not set any ModLogs yet! Your config file will be created when you create the Channel for the first time!");
            }
            modLogs modLogs = new modLogs();
            _modlogsDict.TryGetValue(Context.Guild.Id, out modLogs);

            modLogs.roleChange = !modLogs.roleChange;
            _modlogsDict.TryUpdate(Context.Guild.Id, modLogs);

            ModServiceDB.SaveModLogs(_modlogsDict);
            await Context.Channel.SendMessageAsync($":white_check_mark: Successfully set Role Change log to {modLogs.roleChange}");
        }

        public async Task ToggleServer(CommandContext Context)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You need `Administrator` permissions to change the ModLog config!");
                return;
            }
            if (!_modlogsDict.ContainsKey(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You have not set any ModLogs yet! Your config file will be created when you create the Channel for the first time!");
            }
            modLogs modLogs = new modLogs();
            _modlogsDict.TryGetValue(Context.Guild.Id, out modLogs);

            modLogs.serverChange = !modLogs.serverChange;
            _modlogsDict.TryUpdate(Context.Guild.Id, modLogs);
            ModServiceDB.SaveModLogs(_modlogsDict);
            await Context.Channel.SendMessageAsync($":white_check_mark: Successfully set Server Change log to {modLogs.serverChange}");
        }

        public async Task ToggleChannel(CommandContext Context)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You need `Administrator` permissions to change the ModLog config!");
                return;
            }
            if (!_modlogsDict.ContainsKey(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You have not set any ModLogs yet! Your config file will be created when you create the Channel for the first time!");
            }
            modLogs modLogs = new modLogs();
            _modlogsDict.TryGetValue(Context.Guild.Id, out modLogs);

            modLogs.channelChange = !modLogs.channelChange;
            _modlogsDict.TryUpdate(Context.Guild.Id, modLogs);
            ModServiceDB.SaveModLogs(_modlogsDict);
            await Context.Channel.SendMessageAsync($":white_check_mark: Successfully set Channel Change log to {modLogs.channelChange}");
        }

        public async Task ToggleMessage(CommandContext Context)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You need `Administrator` permissions to change the ModLog config!");
                return;
            }
            if (!_modlogsDict.ContainsKey(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You have not set any ModLogs yet! Your config file will be created when you create the Channel for the first time!");
            }
            modLogs modLogs = new modLogs();
            _modlogsDict.TryGetValue(Context.Guild.Id, out modLogs);

            modLogs.msgDelete = !modLogs.msgDelete;
            _modlogsDict.TryUpdate(Context.Guild.Id, modLogs);
            ModServiceDB.SaveModLogs(_modlogsDict);
            await Context.Channel.SendMessageAsync($":white_check_mark: Successfully set Msg Delete log to {modLogs.msgDelete}");
        }


    }// class

    public class punishStruct
    {
        public ulong channelID { get; set; }
        public List<punishCase> punishes { get; set; }
    }

    public class punishCase
    {
        public int caseNr { get; set; }
        public ModService.Action type { get; set; }
        public string mod { get; set; }
        public ulong modID { get; set; }
        public string user { get; set; }
        public ulong userID { get; set; }
        public string reason { get; set; }
        public ulong punishMsgID { get; set; }
    }

    public class modLogs
    {
        public ulong channelID { get; set; } 
        public bool roleChange { get; set; } = false;
        public bool serverChange { get; set; } = false;
        public bool channelChange { get; set; } = false;
        public bool msgDelete { get; set; } = false; //COOLDOWN
        public DateTime channelChangeInterval { get; set; } = DateTime.UtcNow;
        public DateTime roleChangeInterval { get; set; } = DateTime.UtcNow;
        public DateTime msgDeleteInterval { get; set; } = DateTime.UtcNow;
        public DateTime serverChangeInterval { get; set; } = DateTime.UtcNow;
    }
}

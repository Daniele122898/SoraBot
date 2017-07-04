using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Sora_Bot_1.SoraBot.Services.UserBlacklist
{
    public class BlackListService
    {
        //                          GUILD ,  LIST OF USERS
        private ConcurrentDictionary<ulong, List<ulong>> _guildBlackListDict =
            new ConcurrentDictionary<ulong, List<ulong>>();

        public BlackListService()
        {
            BlackListDB.InitializeLoader();
            var blacklistTemp = BlackListDB.LoadBlackList();
            if (blacklistTemp != null)
            {
                _guildBlackListDict = blacklistTemp;
                Console.WriteLine("LOADED BLACKLIST");
            }
        }


        public async Task BlackListUser(SocketCommandContext Context, SocketGuildUser user)
        {
            try
            {
                var mod = Context.User as SocketGuildUser;
                //var user = userT as SocketGuildUser;
                if (!ModIsAllowed(mod, user, Context).Result)
                    return;

                List<ulong> userList = new List<ulong>();
                if (_guildBlackListDict.ContainsKey(Context.Guild.Id))
                {
                    _guildBlackListDict.TryGetValue(Context.Guild.Id, out userList);
                }

                if (userList == null)
                    userList = new List<ulong>();

                if (userList.Contains(user.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: User is already Blacklisted.");
                    return;
                }

                userList.Add(user.Id);
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: User {user.Username}#{user.DiscriminatorValue} was successfully Blacklisted. He cannot use any of Sora's commands anymore in {Context.Guild.Name}");

                _guildBlackListDict.AddOrUpdate(Context.Guild.Id, userList, (key, oldValue) => userList);
                BlackListDB.SaveBlackList(_guildBlackListDict);
            }
            catch (Exception e)
            {
                await SentryService.SendError(e, Context);
            }
        }

        public bool CheckIfBlacklisted(SocketCommandContext Context)
        {
            if (!_guildBlackListDict.ContainsKey(Context.Guild.Id))
                return false;
            List<ulong> blacklistedUsers = new List<ulong>();
            _guildBlackListDict.TryGetValue(Context.Guild.Id, out blacklistedUsers);
            if (blacklistedUsers == null || blacklistedUsers.Count < 1)
                return false;
            if (blacklistedUsers.Contains(Context.User.Id))
                return true;
            return false;
        }
        private async Task<bool> ModIsAllowed(SocketGuildUser mod, SocketGuildUser user, SocketCommandContext Context)
        {
            try
            {
                if (!mod.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You are not an Administrator :frowning:");
                    return false;
                }
                var modHighestRole = mod.Roles.OrderByDescending(r => r.Position).First();
                //var user = userT as SocketGuildUser;
                var usersHighestRole = user.Roles.OrderByDescending(r => r.Position).First();

                if (usersHighestRole.Position > modHighestRole.Position)
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: You can't Blacklist someone above you in the role hierarchy!");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                await SentryService.SendError(e, Context);
            }
            
            return false;
        }

        public async Task RemoveBlackListUser(SocketCommandContext Context, SocketGuildUser user)
        {
            try
            {
                var mod = Context.User as SocketGuildUser;
                if (!ModIsAllowed(mod, user, Context).Result)
                    return;

                List<ulong> userList = new List<ulong>();
                if (_guildBlackListDict.ContainsKey(Context.Guild.Id))
                {
                    _guildBlackListDict.TryGetValue(Context.Guild.Id, out userList);
                }

                if (userList == null)
                    userList = new List<ulong>();

                if (!userList.Contains(user.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: User is NOT Blacklisted.");
                    return;
                }

                userList.Remove(user.Id);
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: User {user.Username}#{user.DiscriminatorValue} was successfully removed from the Blacklist");

                _guildBlackListDict.AddOrUpdate(Context.Guild.Id, userList, (key, oldValue) => userList);
                BlackListDB.SaveBlackList(_guildBlackListDict);
            }
            catch (Exception e)
            {
                await SentryService.SendError(e, Context);
            }
        }

        public async Task ShowBlackListedUsers(SocketCommandContext Context)
        {
            try
            {
                var mod = Context.User as SocketGuildUser;
                if (!mod.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You are not an Administrator :frowning:");
                    return;
                }

                List<ulong> userList = new List<ulong>();
                if (!_guildBlackListDict.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: There are no Blacklisted users in this guild!");
                    return;
                }
                _guildBlackListDict.TryGetValue(Context.Guild.Id, out userList);
                if (userList == null || userList.Count < 1)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: There are no Blacklisted users in this guild!");
                    return;
                }

                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Title = $"Blacklisted Users in {Context.Guild.Name}",
                    Footer = new EmbedFooterBuilder
                    {
                        IconUrl =  (Context.User.GetAvatarUrl()),
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}"
                    }
                };


                foreach (var userId in userList)
                {
                    var user = Context.Guild.GetUser(userId);
                    if(!user.Discriminator.Equals("0000"))
                        eb.Description += $"{user.Username}#{user.Discriminator}\n";
                }

                await Context.Channel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }
    }
}
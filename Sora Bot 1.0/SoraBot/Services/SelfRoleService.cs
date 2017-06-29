using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;

namespace Sora_Bot_1.SoraBot.Services
{
    public class SelfRoleService
    {
        private ConcurrentDictionary<ulong, List<ulong>> _availableRoles =
            new ConcurrentDictionary<ulong, List<ulong>>();

        private JsonSerializer _jSerializer = new JsonSerializer();

        public SelfRoleService()
        {
            InitializeLoader();
            LoadDatabase();
        }

        public async Task AddRoleToList(SocketCommandContext Context, string roleName)
        {
            //Sora = 270931284489011202
            //Sora test = 276304865934704642
            try
            {
                var sora = Context.Guild.GetUser(270931284489011202);
                var role = Context.Guild.Roles.Where(x => x.Name == roleName).FirstOrDefault();
                //var soraRole = Context.Guild.Roles.Where(x => x.Name == "Sora").FirstOrDefault();
                var soraRole = sora.Roles.OrderByDescending(r => r.Position).FirstOrDefault();
                if (role == null)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Couldn't find specified role!");
                    return;
                }
                if (soraRole == null)
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: Couldn't find my own role! I apparently do not own any roles... Pls report this if it ever happens");
                    await SentryService.SendMessage(
                        $"Couldn't find Soras role? ;_; in {Context.Guild.Name} ({Context.Guild.Id})");
                    return;
                }
                if (soraRole.Position < role.Position)
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: Can't assign Roles that are above me in the role hirachy! **If this is NOT true, open the role hirachy and move any role up once and then back to its initial position! This will update all role positions!**");
                    return;
                }
                if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: Sora needs Manage Roles permissions to add roles!");
                    return;
                }
                Console.WriteLine($"{soraRole.Position} : {role.Position}");

                if (_availableRoles.ContainsKey(Context.Guild.Id))
                {
                    List<ulong> roleIDs = new List<ulong>();
                    _availableRoles.TryGetValue(Context.Guild.Id, out roleIDs);
                    if (roleIDs.Contains(role.Id))
                    {
                        await Context.Channel.SendMessageAsync(":no_entry_sign: This role already is self-assignable");
                        return;
                    }
                    roleIDs.Add(role.Id);
                    _availableRoles.TryUpdate(Context.Guild.Id, roleIDs);
                }
                else
                {
                    List<ulong> roleIDs = new List<ulong>();
                    roleIDs.Add(role.Id);
                    _availableRoles.TryAdd(Context.Guild.Id, roleIDs);
                }
                SaveDatabase();

                await Context.Channel.SendMessageAsync($":white_check_mark: Successfully added Role `{role.Name}`");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Context.Channel.SendMessageAsync(
                    $":no_entry_sign: Failed to add role. I'm probably missing the perms!");
                await SentryService.SendError(e, Context);
            }
        }

        public async Task GetRolesInGuild(SocketCommandContext Context)
        {
            try
            {
                if (!_availableRoles.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: There are no self-assignable roles in this guild!");
                    return;
                }
                List<ulong> roleIDs = new List<ulong>();
                _availableRoles.TryGetValue(Context.Guild.Id, out roleIDs);
                List<ulong> rolesToDelete = new List<ulong>();
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Title = $"Roles in {Context.Guild.Name}",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                        IconUrl = new Uri(Context.User.GetAvatarUrl())
                    },
                    Description = ""
                };
                foreach (var rId in roleIDs)
                {
                    var role = Context.Guild.GetRole(rId);
                    if (role == null)
                    {
                        rolesToDelete.Add(rId);
                        continue;
                    }
                    eb.Description += $"{role.Name}\n";
                }

                if (rolesToDelete.Count > 0)
                {
                    foreach (var role in rolesToDelete)
                    {
                        roleIDs.Remove(role);
                    }
                    _availableRoles.TryUpdate(Context.Guild.Id, roleIDs);
                    SaveDatabase();
                }

                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Context.Channel.SendMessageAsync(
                    $":no_entry_sign: Failed to add role. I'm probably missing the perms!");
                await SentryService.SendError(e, Context);
            }
        }

        public async Task RemoveRoleFromList(SocketCommandContext Context, string roleName)
        {
            try
            {
                if (!_availableRoles.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: There are no self-assignable roles in this guild!");
                    return;
                }

                List<ulong> roleIDs = new List<ulong>();
                _availableRoles.TryGetValue(Context.Guild.Id, out roleIDs);
                bool success = false;
                List<ulong> rolesToDelete = new List<ulong>();
                foreach (var rId in roleIDs)
                {
                    var role = Context.Guild.GetRole(rId);
                    if (role == null)
                    {
                        rolesToDelete.Add(rId);
                        continue;
                    }
                    if (role.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        roleIDs.Remove(rId);
                        if (roleIDs.Count < 1)
                        {
                            List<ulong> ignore = new List<ulong>();
                            _availableRoles.TryRemove(Context.Guild.Id, out ignore);
                        }
                        else
                        {
                            _availableRoles.TryUpdate(Context.Guild.Id, roleIDs);
                        }

                        SaveDatabase();
                        await Context.Channel.SendMessageAsync(
                            $":white_check_mark: Successfully removed Role `{role.Name}`");
                        success = true;
                        break;
                    }
                }

                if (rolesToDelete.Count > 0)
                {
                    foreach (var role in rolesToDelete)
                    {
                        roleIDs.Remove(role);
                    }
                    _availableRoles.TryUpdate(Context.Guild.Id, roleIDs);
                    SaveDatabase();
                }
                if(success)
                    return;

                await Context.Channel.SendMessageAsync(
                    ":no_entry_sign: Specified role could not be found! Use `<prefix>getRoles` to get a list of all self-assignable roles!");
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(
                    $":no_entry_sign: Failed to add role. I'm probably missing the perms!");
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task IAmNotRole(SocketCommandContext Context, string roleName)
        {
            try
            {
                var sora = Context.Guild.GetUser(270931284489011202);
                if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: Sora needs Manage Roles permissions to add roles!");
                    return;
                }
                if (!_availableRoles.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: There are no self-assignable roles in this guild!");
                    return;
                }

                List<ulong> roleIDs = new List<ulong>();
                _availableRoles.TryGetValue(Context.Guild.Id, out roleIDs);
                IRole roleToDel = null;
                List<ulong> rolesToDelete = new List<ulong>();
                foreach (var rId in roleIDs)
                {
                    var role = Context.Guild.GetRole(rId);
                    if (role == null)
                    {
                        rolesToDelete.Add(rId);
                        continue;
                    }
                    if (role.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        roleToDel = role;
                        break;
                    }
                }

                if (rolesToDelete.Count > 0)
                {
                    foreach (var role in rolesToDelete)
                    {
                        roleIDs.Remove(role);
                    }
                    _availableRoles.TryUpdate(Context.Guild.Id, roleIDs);
                    SaveDatabase();
                }

                if (roleToDel == null)
                {
                    await Context.Channel.SendMessageAsync(
                        $"The role `{roleName}` could not be found! Use `<prefix>getRoles` to get a list of all self-assignable roles!");
                    return;
                }

                var soraRole = sora.Roles.OrderByDescending(r => r.Position).FirstOrDefault();
                if (soraRole.Position < roleToDel.Position)
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: Can't remove Roles that are above me (highest role that i have) in the role hirachy! **If this is NOT true, open the role hirachy and move any role up once and then back to its initial position! This will update all role positions!**");
                    return;
                }

                var user = (Context.User as SocketGuildUser);
                bool found = false;
                foreach (var role in user.Roles)
                {
                    if (role.Name == roleName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    await Context.Channel.SendMessageAsync($"You don't have the role `{roleName}`!");
                    return;
                }

                await user.RemoveRoleAsync(roleToDel, null);
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: Successfully removed `{roleToDel.Name}` from your roles!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Context.Channel.SendMessageAsync(
                    $":no_entry_sign: Failed to add role. I'm probably missing the perms!");
                await SentryService.SendError(e, Context);
            }
        }

        public async Task IAmRole(SocketCommandContext Context, string roleName)
        {
            try
            {
                var sora = Context.Guild.GetUser(270931284489011202);
                if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: Sora needs Manage Roles permissions to add roles!");
                    return;
                }
                if (!_availableRoles.ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: There are no self-assignable roles in this guild!");
                    return;
                }

                List<ulong> roleIDs = new List<ulong>();
                _availableRoles.TryGetValue(Context.Guild.Id, out roleIDs);
                IRole roleToAdd = null;
                List<ulong> rolesToDelete = new List<ulong>();
                foreach (var rId in roleIDs)
                {
                    var role = Context.Guild.GetRole(rId);
                    if (role == null)
                    {
                        rolesToDelete.Add(rId);
                        continue;
                    }
                    if (role.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        roleToAdd = role;
                        break;
                    }
                }

                if (rolesToDelete.Count > 0)
                {
                    foreach (var role in rolesToDelete)
                    {
                        roleIDs.Remove(role);
                    }
                    _availableRoles.TryUpdate(Context.Guild.Id, roleIDs);
                    SaveDatabase();
                }

                if (roleToAdd == null)
                {
                    await Context.Channel.SendMessageAsync(
                        $"The role `{roleName}` could not be found! Use `<prefix>getRoles` to get a list of all self-assignable roles!");
                    return;
                }

                var soraRole = sora.Roles.OrderByDescending(r => r.Position).FirstOrDefault();
                if (soraRole.Position < roleToAdd.Position)
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: Can't assign Roles that are above me (highest role that i have) in the role hirachy! **If this is NOT true, open the role hirachy and move any role up once and then back to its initial position! This will update all role positions!**");
                    return;
                }

                var user = (Context.User as SocketGuildUser);

                foreach (var role in user.Roles)
                {
                    if (role.Name.ToLower() == roleName.ToLower())
                    {
                        await Context.Channel.SendMessageAsync(
                            ":no_entry_sign: You already have that role! Use `<prefix>iamnot <rolename>` to get remove a self-assignable role from yourself.");
                        return;
                    }
                }

                await user.AddRoleAsync(roleToAdd, null);
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: Successfully added `{roleToAdd.Name}` to your roles!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Context.Channel.SendMessageAsync(
                    $":no_entry_sign: Failed to add role. I'm probably missing the perms!");
                await SentryService.SendError(e, Context);
            }
        }

        private void InitializeLoader()
        {
            _jSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            _jSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public void SaveDatabase()
        {
            using (StreamWriter sw = File.CreateText(@"selfRoles.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    _jSerializer.Serialize(writer, _availableRoles);
                }
            }
        }

        private void LoadDatabase()
        {
            if (File.Exists("selfRoles.json"))
            {
                using (StreamReader sr = File.OpenText(@"selfRoles.json"))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        var temp = _jSerializer.Deserialize<ConcurrentDictionary<ulong, List<ulong>>>(reader);
                        if (temp == null)
                            return;
                        _availableRoles = temp;
                    }
                }
            }
            else
            {
                File.Create("selfRoles.json").Dispose();
            }
        }
    }
}
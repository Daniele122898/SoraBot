using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Modules.SelfRoleModule
{
    public class SelfRole : ModuleBase
    {
        private SelfRoleService _selfRoleService;
        public SelfRole(SelfRoleService roleService)
        {
            _selfRoleService = roleService;
        }

        [Command("addRole"), Summary("Adds the specified role to the self assignable roles")]
        [Alias("addrole")]
        public async Task addRoleToList([Summary("Name of role to add"), Remainder]string roleName)
        {
            var user = (Context.User as SocketGuildUser);
            if (user.GuildPermissions.Has(GuildPermission.ManageRoles) || user.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await _selfRoleService.AddRoleToList(Context, roleName);
            }
            else
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: To add roles to the self-assignable role list you need `Manage Roles` permissions!");
            }
        }

        [Command("iam"), Summary("Adds the specified role to yourself")]
        [Alias("iAm")]
        public async Task IAmRole([Summary("Name of role to add"), Remainder]string roleName)
        {
            await _selfRoleService.IAmRole(Context, roleName);
        }

        [Command("iamnot"), Summary("Removes the specified role from yourself")]
        [Alias("iAmNot")]
        public async Task IAmNotRole([Summary("Name of role to add"), Remainder]string roleName)
        {
            await _selfRoleService.IAmNotRole(Context, roleName);
        }

        [Command("removeRole"), Summary("Removes the specified role from the self-assignable roles")]
        [Alias("removerole", "delrole")]
        public async Task RemoveRoleFromList([Summary("Name of role to remove"), Remainder]string roleName)
        {
            var user = (Context.User as SocketGuildUser);
            if (user.GuildPermissions.Has(GuildPermission.ManageRoles) || user.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await _selfRoleService.RemoveRoleFromList(Context, roleName);
            }
            else
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: To remove roles from the self-assignable role list you need `Manage Roles` permissions!");
            }
        }

        [Command("getRoles"), Summary("Posts a list of all self-assignable roles in the Guild")]
        [Alias("getroles")]
        public async Task getSelfAssignableRoles()
        {
            await _selfRoleService.GetRolesInGuild(Context);
        }


        [Command("getAllRoles"), Summary("Posts a list of all roles in the Guild")]
        [Alias("getallroles")]
        public async Task getRolesInGuild()
        {
            try
            {
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Title = $"Roles in {Context.Guild.Name}",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Description = ""
                };

                foreach (var r in Context.Guild.Roles.OrderByDescending(r => r.Position))
                {
                    eb.Description += $"{r.Position}. {r.Name}\n";
                }
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

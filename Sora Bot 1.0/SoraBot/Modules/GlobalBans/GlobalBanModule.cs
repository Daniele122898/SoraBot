using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Sora_Bot_1.SoraBot.Services.GlobalSoraBans;

namespace Sora_Bot_1.SoraBot.Modules.GlobalBans
{
    public class GlobalBanModule : ModuleBase<SocketCommandContext>
    {
        private GlobalBanService _banService;

        public GlobalBanModule(GlobalBanService service)
        {
            _banService = service;

        }

        [Command("globalban")]
        [RequireOwner]
        public async Task GlobalBan(SocketUser user, [Remainder] string reason=null)
        {
            await _banService.BanUser(Context,user, reason);
        }
        
        [Command("globalban")]
        [RequireOwner]
        public async Task GlobalBan(ulong Id, [Remainder] string reason=null)
        {
            await _banService.BanUser(Context,Id, reason);
        }
        
        [Command("globalunban")]
        [RequireOwner]
        public async Task GlobalUnBan(SocketUser user)
        {
            await _banService.UnbanUser(Context, user);
        }
        
        [Command("globalbanlist")]
        [RequireOwner]
        public async Task GlobalBanlist()
        {
            await _banService.ShowBannedList(Context);
        }
    }
}
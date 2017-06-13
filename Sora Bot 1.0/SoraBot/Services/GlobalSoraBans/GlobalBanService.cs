using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Sora_Bot_1.SoraBot.Services.GlobalSoraBans
{
    public class GlobalBanService
    {
        private ConcurrentDictionary<ulong, string> _globalBanDict = new ConcurrentDictionary<ulong, string>();
        private string _notDefined = "Not Defined";
        private GlobalBanDB _banDB;

        public GlobalBanService()
        {
            _banDB = GlobalBanDB.Instance;
            _banDB.InitializeLoader();
            _globalBanDict = _banDB.LoadGlobalBanData();
        }

        public async Task BanUser(SocketCommandContext Context, SocketUser user, string reason)
        {
            if (String.IsNullOrWhiteSpace(reason))
                reason = _notDefined;

            if (_globalBanDict.ContainsKey(user.Id))
            {
                if (reason == _notDefined)
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: Can't update a reason without a reason :upside_down:");
                }
                else
                {
                    _globalBanDict.TryUpdate(user.Id, reason);
                    await Context.Channel.SendMessageAsync(
                        $":white_check_mark: Reason has been updated to {reason}");
                }

            }
            else
            {
                _globalBanDict.TryAdd(user.Id, reason);
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: {user.Username}#{user.Discriminator} has been globally banned from using Sora! Reason:\n`{reason}`");
            }
            _banDB.SaveGlobalBanData(_globalBanDict);
        }
        
        public async Task BanUser(SocketCommandContext Context, ulong id, string reason)
        {
            if (String.IsNullOrWhiteSpace(reason))
                reason = _notDefined;

            if (_globalBanDict.ContainsKey(id))
            {
                if (reason == _notDefined)
                {
                    await Context.Channel.SendMessageAsync(
                        ":no_entry_sign: Can't update a reason without a reason :upside_down:");
                }
                else
                {
                    _globalBanDict.TryUpdate(id, reason);
                    await Context.Channel.SendMessageAsync(
                        $":white_check_mark: Reason has been updated to {reason}");
                }

            }
            else
            {
                _globalBanDict.TryAdd(id, reason);
                await Context.Channel.SendMessageAsync(
                    $":white_check_mark: {id} has been globally banned from using Sora! Reason:\n`{reason}`");
            }
            _banDB.SaveGlobalBanData(_globalBanDict);
        }

        public async Task UnbanUser(SocketCommandContext Context, SocketUser user)
        {
            if (!_globalBanDict.ContainsKey(user.Id))
            {
                await Context.Channel.SendMessageAsync(
                    ":no_entry_sign: Can't unban a user that wasn't banned");
                return;
            }

            _globalBanDict.TryRemove(user.Id, out _);
        
            await Context.Channel.SendMessageAsync(
                $":white_check_mark: {user.Username}#{user.Discriminator} has been globally unbanned!");
            
            _banDB.SaveGlobalBanData(_globalBanDict);
        }

        public async Task ShowBannedList(SocketCommandContext Context)
        {
            string send = "```\n";
            foreach (var user in _globalBanDict)
            {
                var pleb = Context.Client.GetUser(user.Key);
                send += $"{(pleb == null ? $"{user.Key}" : $"{pleb.Username}#{pleb.Discriminator}")} :\t\t {user.Value}\n";
            }
            send += "\n```";
            await Context.Channel.SendMessageAsync(send);
        }

        public bool IsBanned(ulong id)
        {
            return _globalBanDict.ContainsKey(id);
        }

        public ConcurrentDictionary<ulong, string> GetGlobalDict()
        {
            return _globalBanDict;
        }
    }
}
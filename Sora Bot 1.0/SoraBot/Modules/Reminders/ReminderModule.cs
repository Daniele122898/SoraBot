using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Sora_Bot_1.SoraBot.Services;
using Sora_Bot_1.SoraBot.Services.Reminder;

namespace Sora_Bot_1.SoraBot.Modules.Reminders
{
    public class ReminderModule : ModuleBase<SocketCommandContext>
    {
        private ReminderService _reminderService;

        public ReminderModule(ReminderService remService)
        {
            _reminderService = remService;
        }

        [Command("remind", RunMode = RunMode.Async),Alias("rem", "rm", "reminder"), Summary("DO THIS")]
        public async Task CreateRemind([Summary("What to remind for"), Remainder] string reminder)
        {
            await _reminderService.SetReminder(Context, reminder);
        }

        [Command("reminders"), Alias("rems", "rms"), Summary("DO THIS")]
        public async Task GetReminders()
        {
            await _reminderService.GetReminders(Context);
        }

        [Command("rmremind", RunMode = RunMode.Async), Alias("rmrem", "rmrm", "rmreminder"), Summary("DO THIS")]
        public async Task RemoveReminder()
        {
            await _reminderService.DelteReminder(Context);
        }

    }
}

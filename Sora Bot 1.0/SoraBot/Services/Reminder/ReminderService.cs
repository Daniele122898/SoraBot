using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;

namespace Sora_Bot_1.SoraBot.Services.Reminder
{
    public class ReminderService
    {
        private Timer _timer;
        private DiscordSocketClient _client;
        private ConcurrentDictionary<ulong, List<RemindData>> _remindDict = new ConcurrentDictionary<ulong, List<RemindData>>();
        private InteractiveService _interactiveService;

        private const int INITIAL_DELAY = 40;

        public ReminderService(DiscordSocketClient client, InteractiveService inter)
        {
            try
            {
                _client = client;
                _interactiveService = inter;

                ReminderDB.InitializeLoader();
                var reminders = ReminderDB.LoadReminders();
                if (reminders != null)
                {
                    _remindDict = reminders;
                    Console.WriteLine("LOADED REMINDER DICT");
                }
                
                Task.Factory.StartNew(() => { InitializeTimer(); });

                //ChangeToClosestInterval(); DELAY THE FIRST RUN TO THE INITIAL DELAY!
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }


        public async Task InitializeTimer()
        {
            try
            {
                _timer = new Timer(async _ =>
                {////await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync("",false,eb);
                    foreach (var user in _remindDict.ToArray())
                    {
                        List<RemindData> itemsToRemove = new List<RemindData>();
                        foreach (var reminder in user.Value)
                        {
                            if (reminder.TimeToRemind.CompareTo(DateTime.UtcNow) <= 0)
                            {
                                var userToRemind = _client.GetUser(user.Key);
                                await (await userToRemind.GetOrCreateDMChannelAsync()).SendMessageAsync($":alarm_clock: **Reminder:** {reminder.message}");
                                itemsToRemove.Add(reminder);
                            }
                        }
                        foreach (var remove in itemsToRemove)
                        {
                            user.Value.Remove(remove);
                        }
                        _remindDict.TryUpdate(user.Key, user.Value);
                    }
                    ChangeToClosestInterval();
                    ReminderDB.SaveReminders(_remindDict);
                },
                null,
                TimeSpan.FromSeconds(INITIAL_DELAY),// Time that message should fire after bot has started
                TimeSpan.FromSeconds(INITIAL_DELAY)); //time after which message should repeat (timout.infinite for no repeat)

                Console.WriteLine("TIMER INITIALIZED");

                /*if(str.timeToTriggerAgain.CompareTo(DateTime.UtcNow) >0)
                        {
                            return;
                        }*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e);
            }
        }

        /*
        public void Stop() //Example to make the timer stop running
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart() //Example to restart the timer
        {
            _timer.Change(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(30));
        }*/

        public async Task DelteReminder(SocketCommandContext Context)
        {
            try
            {
                if (!_remindDict.ContainsKey(Context.User.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You have no Reminders!");
                    return;
                }
                List<RemindData> data = new List<RemindData>();
                _remindDict.TryGetValue(Context.User.Id, out data);

                if (data.Count < 1)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You have no Reminders!");
                    return;
                }

                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    Title = "Enter the Index of the Reminder you want to remove.",
                };

                string reminders = "";
                int count = 1;
                foreach (var v in data)
                {
                    reminders += $"**{count}.** {v.message}\n";
                    count++;
                }
                reminders += $"**{count}.** Cancel";
                eb.Description = reminders;
                var del = await Context.Channel.SendMessageAsync("", embed: eb);
                var response = await _interactiveService.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(20));
                await del.DeleteAsync();
                if (response == null)
                {
                    await Context.Channel.SendMessageAsync($":no_entry_sign: Answer timed out {Context.User.Mention} (≧д≦ヾ)");
                    return;
                }
                int index = 0;
                if (!Int32.TryParse(response.Content, out index))
                {

                    await Context.Channel.SendMessageAsync($":no_entry_sign: Only add the Index");
                    return;
                }
                if (index > (data.Count + 1) || index < 1)
                {
                    await Context.Channel.SendMessageAsync($":no_entry_sign: Invalid Number");
                    return;
                }
                if (index == count)
                {
                    await Context.Channel.SendMessageAsync($":no_entry_sign: Action Cancelled");
                    return;
                }
                index -= 1;
                var msgThatGetsRemoved = data[index].message;
                data.RemoveAt(index);
                _remindDict.TryUpdate(Context.User.Id, data);
                ReminderDB.SaveReminders(_remindDict);
                await Context.Channel.SendMessageAsync($":white_check_mark: Successfully removed Reminder: `{msgThatGetsRemoved}`");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public async Task GetReminders(SocketCommandContext Context)
        {
            try
            {
                if (!_remindDict.ContainsKey(Context.User.Id))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You have no Reminders!");
                    return;
                }
                List<RemindData> data = new List<RemindData>();
                _remindDict.TryGetValue(Context.User.Id, out data);

                if (data.Count < 1)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You have no Reminders!");
                    return;
                }

                var orderedList = data.OrderBy(x => x.TimeToRemind).ToList();

                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    ThumbnailUrl = new Uri(Context.User.GetAvatarUrl()),
                    Title = $"{Context.User.Username}, your Reminders are"
                };
                for (int i = 0; i < orderedList.Count && i<10; i++)
                {
                    eb.AddField((x) =>
                    {
                        x.Name = $"Reminder #{i + 1} in {ConvertTime(orderedList[i].TimeToRemind.Subtract(DateTime.UtcNow).TotalSeconds)}";
                        x.IsInline = false;
                        x.Value = $"{orderedList[i].message}";
                    });
                }

                await Context.Channel.SendMessageAsync("", embed: eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public bool CheckTimeInterval(SocketUser user, double time)
        {
            try
            {
                if (!_remindDict.ContainsKey(user.Id))
                    return true;
                List<RemindData> dataList = new List<RemindData>();
                _remindDict.TryGetValue(user.Id, out dataList);

                double closestTime = Double.PositiveInfinity;

                var dateTimeToFind = DateTime.UtcNow.AddSeconds(time);

                foreach (var reminder in dataList)
                {
                    var delta = Math.Abs(reminder.TimeToRemind.Subtract(dateTimeToFind).TotalSeconds);
                    if (delta < 0)
                        delta = 0;
                    if (closestTime > delta)
                        closestTime = delta;
                }
                if (closestTime <= 60)
                    return false;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        public async Task SetReminder(SocketCommandContext Context, string message)
        {
            try
            {
                var time = GetTimeInterval(message);
                if (time == 0)
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: Your time format was incorrect!");
                    return;
                }
                if(!CheckTimeInterval(Context.User, time))
                {
                    await Context.Channel.SendMessageAsync(":no_entry_sign: You cannot have 2 Reminders that are within 60 seconds of each other");
                    return;
                }

                List<RemindData> dataList = new List<RemindData>();
                if (_remindDict.ContainsKey(Context.User.Id))
                {
                    _remindDict.TryGetValue(Context.User.Id, out dataList);
                }
                message = message.Replace("mins", "m");
                message = message.Replace("minutes", "m");
                message = message.Replace("minute", "m");
                message = message.Replace("min", "m");

                var msg = message.Substring(0,message.LastIndexOf("in"));
                RemindData data = new RemindData
                {
                    TimeToRemind = DateTime.UtcNow.AddSeconds(time),
                    message = msg
                };
                dataList.Add(data); //_punishLogs.AddOrUpdate(Context.Guild.Id, str, (key, oldValue) => str);
                _remindDict.AddOrUpdate(Context.User.Id, dataList,
                    (key, oldValue) => dataList);
                ChangeToClosestInterval();
                ReminderDB.SaveReminders(_remindDict);
                await Context.Channel.SendMessageAsync($":white_check_mark: Successfully set Reminder. I will remind you to `{data.message}` in `{message.Substring(message.LastIndexOf("in")+2)}`!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, Context);
            }
        }

        public double GetTimeInterval(string message)
        {
            try
            {
                if (!message.Contains("in"))
                {
                    Console.WriteLine("NO IN");
                    return 0;
                }
                string[] seperator = new string[]
                {
                    "in"
                };
                message = message.Replace("mins", "m");
                message = message.Replace("minutes", "m");
                message = message.Replace("minute", "m");
                message = message.Replace("min", "m");

                var msg = message.Substring(message.LastIndexOf("in") + 2);

                var regex = Regex.Matches(msg, @"(\d+)\s{0,1}([a-zA-Z]*)");
                double timeToAdd = 0;
                for (int i = 0; i < regex.Count; i++)
                {
                    var captures = regex[i].Groups;
                    if (captures.Count < 3)
                    {
                        Console.WriteLine("CAPTURES COUNT LESS THEN 3");
                        return 0;
                    }

                    double amount = 0;

                    if (!Double.TryParse(captures[1].ToString(), out amount))
                    {
                        Console.WriteLine($"COULDNT PARSE DOUBLE : {captures[1].ToString()}");
                        return 0;
                    }

                    switch (captures[2].ToString())
                    {
                        case ("weeks"):
                        case ("week"):
                        case ("w"):
                            timeToAdd += amount * 604800;
                            break;
                        case ("day"):
                        case ("days"):
                        case ("d"):
                            timeToAdd += amount * 86400;
                            break;
                        case ("hours"):
                        case ("hour"):
                        case ("h"):
                            timeToAdd += amount * 3600;
                            break;
                        case ("minutes"):
                        case ("minute"):
                        case ("m"):
                        case ("min"):
                        case ("mins"):
                            timeToAdd += amount * 60;
                            break;
                        case ("seconds"):
                        case ("second"):
                        case ("s"):
                            timeToAdd += amount;
                            break;
                        default:
                            Console.WriteLine("SWITCH FAILED");
                            return 0;
                            break;
                    }
                }
                return timeToAdd;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return 0;
        }

        public string ConvertTime(double value)
        {
            TimeSpan ts = TimeSpan.FromSeconds(value);
            if (value > 86400)
            {
                return String.Format("{0}d {1}h {2}m {3:D2}s", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            }
            else if (value > 3600)
            {
                return String.Format("{0}h {1}m {2:D2}s", ts.Hours, ts.Minutes, ts.Seconds);
            }
            else if (value > 60)
            {
                return String.Format("{0}m {1:D2}s", ts.Minutes, ts.Seconds);
            }
            return String.Format("{0:D2}s", ts.Seconds);
        }

        public void ChangeToClosestInterval()
        {
            double timeToUpdate = Double.PositiveInfinity;
            foreach (var user in _remindDict)
            {
                foreach (var reminder in user.Value)
                {
                    var delta = reminder.TimeToRemind.Subtract(DateTime.UtcNow).TotalSeconds;
                    if (delta < 0)
                        delta = 0;
                    if (timeToUpdate > delta)
                        timeToUpdate = delta;
                }
            }

            if (Double.IsPositiveInfinity(timeToUpdate))
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine($"TIMER HAS BEEN HALTED!");
            }
            else
            {
                _timer.Change(TimeSpan.FromSeconds(timeToUpdate), TimeSpan.FromSeconds(timeToUpdate));
                Console.WriteLine($"CHANGED TIMER INTERVAL TO: {timeToUpdate}");
            }
        }
    }

    public class RemindData
    {
        public DateTime TimeToRemind;
        public string message;
    }

}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Sora_Bot_1.SoraBot.Services
{
    public class RatelimitService
    {
        ConcurrentDictionary<ulong, userRate> userLimiterDict = new ConcurrentDictionary<ulong, userRate>();
        private int timeToAdd = 2000;
        private int punishTime = 15000;


        public async Task checkRatelimit(CommandContext context)
        {
            try
            {
                if (userLimiterDict.ContainsKey(context.User.Id))
                {
                    userRate userStruct = new userRate();
                    userLimiterDict.TryGetValue(context.User.Id, out userStruct);
                    //CHECK TIME
                    if (Environment.TickCount < userStruct.timeBetween)
                    {
                        userStruct.counter += 1;
                        if (userStruct.counter >= 4)
                        {
                            //ratelimit
                            if (!userStruct.messageSent)
                            {
                                userStruct.timeBetween = Environment.TickCount + timeToAdd;
                                await (await context.User.CreateDMChannelAsync()).SendMessageAsync(
                                    "**You have been ratelimited for 20 seconds. Please do not spam commands! If you continue doing so your lockout will increase in time!**\n" +
                                    "If this was by mistake and you did not spam. join https://discord.gg/Pah4yj5 and @ me");
                                userStruct.timeBetween += punishTime;
                                userStruct.messageSent = true;
                            }
                            userStruct.timeBetween += timeToAdd;
                            userLimiterDict.TryUpdate(context.User.Id, userStruct);
                        }
                        else
                        {
                            //add time
                            userStruct.timeBetween = Environment.TickCount + timeToAdd;
                            userLimiterDict.TryUpdate(context.User.Id, userStruct);
                        }
                    }
                    else
                    {
                        //reset Ratelimit
                        userStruct.counter = 1;
                        userStruct.timeBetween = Environment.TickCount + timeToAdd;
                        userStruct.messageSent = false;
                        userLimiterDict.TryUpdate(context.User.Id, userStruct);
                    }
                }
                else
                {
                    userRate userStruct = new userRate
                    {
                        counter = 1,
                        timeBetween = Environment.TickCount + timeToAdd,
                        messageSent = false
                    };
                    userLimiterDict.TryAdd(context.User.Id, userStruct);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, context);
            }
        }

        public async Task<bool> onlyCheck(CommandContext context)
        {
            try
            {
                if (userLimiterDict.ContainsKey(context.User.Id))
                {
                    userRate userStruct = new userRate();
                    userLimiterDict.TryGetValue(context.User.Id, out userStruct);
                    //CHECK TIME
                    if (Environment.TickCount < userStruct.timeBetween)
                    {
                        if (userStruct.counter >= 5)
                        {
                            //ratelimit
                            if (!userStruct.messageSent)
                            {
                                userStruct.timeBetween = Environment.TickCount + timeToAdd;
                                await (await context.User.CreateDMChannelAsync()).SendMessageAsync(
                                    "**You have been ratelimited for 20 seconds. Please do not spam commands! If you continue doing so your lockout will increase in time!**\n" +
                                    "If this was by mistake and you did not spam. join https://discord.gg/Pah4yj5 and @ me");
                                userStruct.timeBetween += punishTime;
                                userStruct.messageSent = true;
                                await SentryService.SendMessage($"Rate limit occured:\n" +
                                                                $"User: {context.User.Username}#{context.User.Discriminator} \t{context.User.Id}\n" +
                                                                $"Guild: {context.Guild.Name} \t {context.Guild.Id}\n" +
                                                                $"Message: {context.Message.Content}");
                            }
                            userStruct.timeBetween += timeToAdd;
                            userLimiterDict.TryUpdate(context.User.Id, userStruct);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SentryService.SendError(e, context);
            }
            return false;
        }

        public struct userRate
        {
            public int counter;
            public int timeBetween;
            public bool messageSent;
        }
    }
}
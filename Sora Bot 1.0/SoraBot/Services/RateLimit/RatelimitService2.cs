using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sora_Bot_1.SoraBot.Services.RateLimit
{
    public class RatelimitService2
    {
        private const int BUCKET_INITIAL_FILL = 16;
        private const int BUCKET_MAX_FILL = 32;
        private const double BUCKET_DROP_INTERVAL = 10;
        private const int BUCKET_DROP_SIZE = 1;
        private const int BUCKET_DRAIN_SIZE = 1;
        private const int BUCKET_RATELIMITER = 20;
        private const int COMBO_MAX = 5;
        private const int COMBO_PENALTY = 12;
        private const int COMBO_TIME_INTERVALL = 3;
        private const int COMBO_RATELIMITER = 30;

        private ConcurrentDictionary<ulong, BucketData> _bucketDict = new ConcurrentDictionary<ulong, BucketData>();
        private Timer _timer;

        public RatelimitService2()
        {
            //LOAD DB
            Task.Factory.StartNew(() => { InitializeBucketRefillTimer(); });
        }

        private async Task InitializeBucketRefillTimer()
        {
            try
            {
                _timer = new Timer(_ =>
                {////await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync("",false,eb);
                    var temp = _bucketDict.ToArray();
                    foreach (var bucket in temp)
                    {
                        if (bucket.Value.bucketSize < BUCKET_MAX_FILL)
                            bucket.Value.bucketSize += BUCKET_DROP_SIZE;
                        _bucketDict.TryUpdate(bucket.Key, bucket.Value);
                    }
                    //SAVE DB
                },
               null,
               TimeSpan.FromSeconds(BUCKET_DROP_INTERVAL),// Time that message should fire after bot has started
               TimeSpan.FromSeconds(BUCKET_DROP_INTERVAL)); //time after which message should repeat (timout.infinite for no repeat)

                Console.WriteLine("BUCKET REFILL INITIALIZED");

            }
            catch (Exception e)
            {
                await SentryService.SendError(e);
            }
        }

        private void CreateBucketIfNotExistant(SocketUser user)
        {
            try
            {
                if (!_bucketDict.ContainsKey(user.Id))
                {
                    BucketData data = new BucketData
                    {
                        bucketSize = BUCKET_INITIAL_FILL,
                        lastCommand = DateTime.UtcNow,
                        combo = 0,
                        rateLimitedTill = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(10))
                    };
                    _bucketDict.TryAdd(user.Id, data);
                    //TODO SAVE DATABASE
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                SentryService.SendError(e);
            }
        }

        public async Task RateLimitMain(SocketUser user)
        {
            try
            {
                CreateBucketIfNotExistant(user);
                if (!CheckComboAndAdd(user).Result)
                {
                    return;
                }
                await SubstractFromBucket(user);
            }
            catch (Exception e)
            {
                await SentryService.SendError(e);
            }
        }

        public bool CheckIfRatelimited(SocketUser user)
        {
            try
            {
                BucketData data = new BucketData();
                if (!_bucketDict.TryGetValue(user.Id, out data))
                    return false;
                if (data.rateLimitedTill.CompareTo(DateTime.UtcNow) >= 0)
                    return true;
                if (data.bucketSize <= 0)
                    return true;
               // if (data.combo >= COMBO_MAX)
              //      return true;
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                SentryService.SendError(e);
            }
            return false;
        }

        private async Task SubstractFromBucket(SocketUser user)
        {
            try
            {
                BucketData data = new BucketData();
                if (!_bucketDict.TryGetValue(user.Id, out data))
                    return;
                data.bucketSize -= BUCKET_DRAIN_SIZE;
                if (data.bucketSize < 0)
                {
                    //RATELIMIT
                    data.rateLimitedTill = DateTime.UtcNow.AddSeconds(BUCKET_RATELIMITER);
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("" +
                            $"**You have been ratelimited for {BUCKET_RATELIMITER} seconds. Please do not spam commands or stars! If you continue doing so your lockout will increase in time!**\n" +
                                        "If this was by mistake and you did not spam. join https://discord.gg/Pah4yj5 and @ me");
                    await SentryService.SendMessage($"**Rate limit occured:**\n" +
                                                                    $"User: {user.Username}#{user.Discriminator} \t{user.Id}");
                }
                _bucketDict.TryUpdate(user.Id, data);
                //TODO SAVE DATABASE
            }
            catch (Exception e)
            {
                await SentryService.SendError(e);
            }   
        }

        private async Task<bool> CheckComboAndAdd(SocketUser user)
        {
            try
            {
                BucketData data = new BucketData();
                if (!_bucketDict.TryGetValue(user.Id, out data))
                    return false;
                //Check if still in combo time
                if (DateTime.UtcNow.Subtract(data.lastCommand).TotalSeconds <= COMBO_TIME_INTERVALL)
                {
                    //COMBO BREAKER IF NEEDED
                    data.combo += 1;
                    data.lastCommand = DateTime.UtcNow;
                    if (data.combo >= COMBO_MAX)
                    {
                        data.rateLimitedTill = DateTime.UtcNow.AddSeconds(COMBO_RATELIMITER);
                        await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("" +
                            $"**You have been ratelimited for {COMBO_RATELIMITER} seconds. Your ratelimit was triggered by the combo breaker and thus your time is higher than normal! Please do not spam commands or stars! If you continue doing so your lockout will increase in time!**\n" +
                                        "If this was by mistake and you did not spam. join https://discord.gg/Pah4yj5 and @ me");
                        await SentryService.SendMessage($"**Rate limit occured:**\n" +
                                                                    $"User: {user.Username}#{user.Discriminator} \t{user.Id}");
                        data.bucketSize -= COMBO_PENALTY;
                        _bucketDict.TryUpdate(user.Id, data);
                        //TODO SAVE DATABASE
                        return false;
                    }
                    _bucketDict.TryUpdate(user.Id, data);
                    //TODO SAVE DATABASE
                    return true;
                }
                //Reset combo
                data.lastCommand = DateTime.UtcNow;
                data.combo = 1;
                _bucketDict.TryUpdate(user.Id, data);
                //TODO SAVE DATABASE
                return true;
            }
            catch (Exception e)
            {
                await SentryService.SendError(e);
            }
            return false;   
        }

    }

    public class BucketData
    {
        public int bucketSize { get; set; }
        public DateTime lastCommand { get; set; }
        public int combo { get; set; }
        public DateTime rateLimitedTill { get; set; }
    }
}

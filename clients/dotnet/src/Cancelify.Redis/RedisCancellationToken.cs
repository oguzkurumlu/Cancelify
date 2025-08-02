using Cancelify.Core;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Cancelify.Redis
{
    public class RedisCancellationToken : IDistributedCancellationToken, IDisposable
    {
        private readonly ISubscriber _subscriber;
        private readonly string _channelPrefix;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokenSources = new ConcurrentDictionary<string, CancellationTokenSource>();
        private readonly IRedisConnection _redis;

        public RedisCancellationToken(IRedisConnection redis, string channelPrefix = "cancel-token:")
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _subscriber = _redis.GetSubscriber();
            _channelPrefix = channelPrefix;

            _subscriber.Subscribe(
                new RedisChannel($"{_channelPrefix}*", RedisChannel.PatternMode.Pattern),
                (channel, message) =>
                {
                    string key = channel.ToString().Replace(_channelPrefix, "");
                    if (_tokenSources.TryRemove(key, out var cts))
                    {
                        cts.Cancel();
                        cts.Dispose();
                    }
                });
        }

        public CancellationToken GetToken(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return _tokenSources.GetOrAdd(id, _ => new CancellationTokenSource()).Token;
        }

        public async Task CancelAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            await _subscriber.PublishAsync($"{_channelPrefix}{id}", "cancel");
        }

        public void Dispose()
        {
            foreach (var kv in _tokenSources)
            {
                kv.Value.Dispose();
            }

            _redis.Dispose();
        }
    }

}

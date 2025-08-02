using Cancelify.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Cancelify.Redis
{
    public static class Extension
    {
        public static IServiceCollection AddRedisCancelify(this IServiceCollection services, string redisConnectionString, string channelPrefix = "cancel-token:")
        {
            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                throw new ArgumentException("Redis connection string must be provided.");
            }
            services.AddSingleton<IDistributedCancellationToken>(sp =>
                new RedisCancellationToken(redisConnectionString, channelPrefix));
            return services;
        }
    }
}

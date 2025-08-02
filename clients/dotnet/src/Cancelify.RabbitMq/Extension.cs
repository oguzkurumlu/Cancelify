using Cancelify.Core;
using Cancelify.RabbitMq;
using Microsoft.Extensions.DependencyInjection;

namespace Cancelify.Redis
{
    public static class Extension
    {
        public static IServiceCollection AddRabbitMqCancelify(this IServiceCollection services, string rabbitMqUri, string channelPrefix = "cancel-token:")
        {
            if (string.IsNullOrWhiteSpace(rabbitMqUri))
            {
                throw new ArgumentException("Redis connection string must be provided.");
            }
            services.AddSingleton<IDistributedCancellationToken>(sp =>
                new RabbitMqCancellationToken(new DefaultRabbitMqConnectionFactory(rabbitMqUri), channelPrefix));

            return services;
        }
    }
}

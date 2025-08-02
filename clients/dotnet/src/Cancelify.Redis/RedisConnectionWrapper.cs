using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cancelify.Redis
{
    public class RedisConnectionWrapper : IRedisConnection
    {
        private readonly ConnectionMultiplexer _connection;

        public RedisConnectionWrapper(string connectionString)
        {
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public ISubscriber GetSubscriber()
        {
            return _connection.GetSubscriber();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

}

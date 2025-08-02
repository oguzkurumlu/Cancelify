using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cancelify.Redis
{
    public interface IRedisConnection : IDisposable
    {
        ISubscriber GetSubscriber();
    }
}

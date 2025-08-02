using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cancelify.RabbitMq
{
    public class DefaultRabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private readonly ConnectionFactory _factory;

        public DefaultRabbitMqConnectionFactory(string uri)
        {
            _factory = new ConnectionFactory
            {
                Uri = new Uri(uri),
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
            };
        }

        public IConnection CreateConnection() => _factory.CreateConnection();
    }

}

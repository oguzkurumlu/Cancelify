using Cancelify.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace Cancelify.RabbitMq
{
    public class RabbitMqCancellationToken : IDistributedCancellationToken
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokenSources = new();

        public RabbitMqCancellationToken(IRabbitMqConnectionFactory factory, string exchangeName = "cancel-token")
        {
            _exchangeName = exchangeName;

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Fanout, durable: false, autoDelete: true);

            _queueName = _channel.QueueDeclare(queue: "", durable: false, exclusive: true, autoDelete: true).QueueName;
            _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceived;

            _channel.BasicConsume(queue: _queueName, autoAck: true, (IBasicConsumer)consumer);
        }

        private Task OnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            var id = Encoding.UTF8.GetString(args.Body.ToArray());

            if (!string.IsNullOrWhiteSpace(id))
            {
                if (_tokenSources.TryRemove(id, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }
            }

            return Task.CompletedTask;
        }

        public CancellationToken GetToken(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return _tokenSources.GetOrAdd(id, _ => new CancellationTokenSource()).Token;
        }

        public Task CancelAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var message = Encoding.UTF8.GetBytes(id);
            _channel.BasicPublish(exchange: _exchangeName, routingKey: "", body: message);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var cts in _tokenSources.Values)
            {
                cts.Dispose();
            }

            _channel?.Close();
            _connection?.Close();
        }
    }
}

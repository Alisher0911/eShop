using System;
using System.Data.SqlClient;
using System.Text;
using AutoMapper;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Common;
using EventBusRabbitMQ.Events;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;
using Ordering.Infrastructure.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ordering.API.RabbitMQ
{
    public class EventBusRabbitMQConsumer
    {
        private readonly IRabbitMQConnection _connection;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _scopeFactory;

        public EventBusRabbitMQConsumer(IRabbitMQConnection connection, IMapper mapper, IServiceScopeFactory scopeFactory)
        {
            _connection = connection;
            _mapper = mapper;
            _scopeFactory = scopeFactory;
        }

        public void Consume()
        {
            var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: EventBusConstants.BasketCheckoutQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnReceivedEvent;

            channel.BasicConsume(queue: EventBusConstants.BasketCheckoutQueue, autoAck: true, consumer: consumer);
        }

        private async void OnReceivedEvent(object sender, BasicDeliverEventArgs e)
        {
            if (e.RoutingKey == EventBusConstants.BasketCheckoutQueue)
            {
                var message = Encoding.UTF8.GetString(e.Body.Span);
                Console.WriteLine(" [x] Received {0}", message);
                var basketCheckout = JsonConvert.DeserializeObject<BasketCheckoutEvent>(message);

                var order = _mapper.Map<Order>(basketCheckout);
                if (order == null)
                {
                    throw new ApplicationException("Could not be mapped.");
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                    await orderRepository.AddAsync(order);
                }
            }
        }

        public void Disconnect()
        {
            _connection.Dispose();
        }
    }
}

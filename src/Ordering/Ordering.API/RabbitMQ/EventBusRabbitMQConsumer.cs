using System;
using System.Text;
using AutoMapper;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Common;
using Ordering.Core.Entities;
using Ordering.Infrastructure.Data;
using Ordering.Infrastructure.Repositories.Base;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ordering.API.RabbitMQ
{
    public class EventBusRabbitMQConsumer : Repository<Order>
    {
        private readonly IRabbitMQConnection _connection;
        private readonly IMapper _mapper;

        public EventBusRabbitMQConsumer(IRabbitMQConnection connection, IMapper mapper, OrderContext dbContext) : base(dbContext)
        {
            _connection = connection;
            _mapper = mapper;
        }

        public void Consume()
        {
            var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: EventBusConstants.BasketCheckoutQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnReceivedEvent;

            channel.BasicConsume(queue: EventBusConstants.BasketCheckoutQueue, autoAck: true, consumer: consumer);
        }

        private void OnReceivedEvent(object sender, BasicDeliverEventArgs e)
        {
            if (e.RoutingKey == EventBusConstants.BasketCheckoutQueue)
            {
                throw new NotImplementedException();
                /*var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);

                var order = _mapper.Map<Order>(message);
                _dbContext.Orders.AddAsync(order);
                _dbContext.SaveChangesAsync();*/
            }
        }

        public void Disconnect()
        {
            _connection.Dispose();
        }
    }
}

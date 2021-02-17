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

        private void OnReceivedEvent(object sender, BasicDeliverEventArgs e)
        {
            if (e.RoutingKey == EventBusConstants.BasketCheckoutQueue)
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
                var basketCheckout = JsonConvert.DeserializeObject<BasketCheckoutEvent>(message);

                var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderContext>();
                var order = new Order
                {
                    Username = basketCheckout.Username,
                    TotalPrice = basketCheckout.TotalPrice,

                    FirstName = basketCheckout.FirstName,
                    LastName = basketCheckout.LastName,
                    EmailAddress = basketCheckout.EmailAddress,
                    AddressLine = basketCheckout.AddressLine,
                    Country = basketCheckout.Country,
                    State = basketCheckout.State,
                    ZipCode = basketCheckout.ZipCode,

                    CardName = basketCheckout.CardName,
                    CardNumber = basketCheckout.CardNumber,
                    Expiration = basketCheckout.Expiration,
                    CVV = basketCheckout.CVV,
                    PaymentMethod = (PaymentMethod)basketCheckout.PaymentMethod
                };

                db.Orders.Add(order);
                db.SaveChanges();
            }
        }

        public void Disconnect()
        {
            _connection.Dispose();
        }
    }
}

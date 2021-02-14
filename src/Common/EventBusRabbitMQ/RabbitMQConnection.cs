using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EventBusRabbitMQ
{
    public class RabbitMQConnection : IRabbitMQConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private bool _disposed;

        public RabbitMQConnection(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            if (!IsConnected)
            {
                TryConnect();
            }
        }



        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }


        public bool TryConnect()
        {
            try
            {
                _connection = _connectionFactory.CreateConnection();
            } catch(BrokerUnreachableException)
            {
                Thread.Sleep(2000);
                _connection = _connectionFactory.CreateConnection();
            }

            if (IsConnected)
            {
                Console.WriteLine("RabbitMQ connection acqured");
                return true;
            } else
            {
                Console.WriteLine("Error: RabbitMQ connection failed");
                return false;
            }
        }


        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Error: RabbitMQ Connection is not open");
            }
            return _connection.CreateModel();
        }


        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                _connection.Dispose();
                _disposed = true;
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

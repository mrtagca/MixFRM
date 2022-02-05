using MixFRM.Queue.RabbitMQ.Types;
using MixFRM.Utils.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Queue.RabbitMQ
{
    public static class RabbitMQPublishManager
    {

        private static ConnectionFactory _factory;
        static RabbitMQPublishManager()
        {
            _factory = new ConnectionFactory()
            {
                HostName = RabbitMQConfigurations.HostName,
                UserName = RabbitMQConfigurations.UserName,
                Password = RabbitMQConfigurations.Password
            };
        }

        public static void BasicPublish(object data, QueueOptions queueOptions, PublishOptions publishOptions)
        {


            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: queueOptions.QueueName,
                    durable: queueOptions.Durable,
                    exclusive: queueOptions.Exclusive,
                    autoDelete: queueOptions.AutoDelete,
                    arguments: queueOptions.Arguments == null ? null : queueOptions.Arguments
                    );

                var message = FrmJsonSerializer.Serialize(data);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: publishOptions == null ? "" : publishOptions.Exchange,
                                   routingKey: queueOptions.QueueName,
                                   basicProperties: publishOptions == null ? null : publishOptions.BasicProperties,
                                   body: body);

            }
        }
    }
}

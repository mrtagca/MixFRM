using MixFRM.Queue.RabbitMQ.Types;
using MixFRM.Utils.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Queue.RabbitMQ
{
    public static class RabbitMQConsumeManager
    {
        private static ConnectionFactory _factory;
        public static IModel _channel;
        static RabbitMQConsumeManager()
        {
            _factory = new ConnectionFactory()
            {
                HostName = RabbitMQConfigurations.HostName,
                UserName = RabbitMQConfigurations.UserName,
                Password = RabbitMQConfigurations.Password
            };
        }

        public static void BasicConsume(QueueOptions queueOptions, bool autoAck = false, EventHandler<BasicDeliverEventArgs> eventHandler = null)
        {
            using (IConnection connection = _factory.CreateConnection())
            //using (IModel channel = connection.CreateModel())
            using (_channel = connection.CreateModel())
            {
                _channel.QueueDeclare(
                     queue: queueOptions.QueueName,
                     durable: queueOptions.Durable,
                     exclusive: queueOptions.Exclusive,
                     autoDelete: queueOptions.AutoDelete,
                     arguments: queueOptions.Arguments == null ? null : queueOptions.Arguments
                     );

                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += new EventHandler<BasicDeliverEventArgs>(eventHandler);

                _channel.BasicConsume(queue: "test2",
                                     autoAck: autoAck,//true ise mesaj otomatik olarak kuyruktan silinir
                                     consumer: consumer);
                Console.ReadKey();
            }
        }

        //public static void Test(object? model, BasicDeliverEventArgs mq)
        //{
        //    var body = mq.Body.ToArray();
        //    var message = Encoding.UTF8.GetString(body);
        //    string email = FrmJsonSerializer.Deserialize<string>(message);


        //    ulong deliveryTag = mq.DeliveryTag;
        //    _channel.BasicAck(deliveryTag, false);

        //    Console.WriteLine($"Mesaj : {email}");
        //}
    }
}

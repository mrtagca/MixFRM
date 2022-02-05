using MixFRM.Queue.RabbitMQ;
using MixFRM.Queue.RabbitMQ.Types;
using MixFRM.Utils.Json;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Test.RabbitMQ.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            QueueOptions options = new QueueOptions()
            {
                QueueName = "test2"
            };

            EventHandler<BasicDeliverEventArgs> eventHandler = new
                 EventHandler<BasicDeliverEventArgs>(Test);
            RabbitMQConsumeManager.BasicConsume(options,false, eventHandler);
        }

        public static void Test(object? model, BasicDeliverEventArgs mq)
        {
            var body = mq.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            string email = FrmJsonSerializer.Deserialize<string>(message);


            ulong deliveryTag = mq.DeliveryTag;
            RabbitMQConsumeManager._channel.BasicAck(deliveryTag, false);

            Console.WriteLine($"Mesaj : {email}");
        }
    }
}

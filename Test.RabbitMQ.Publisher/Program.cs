using MixFRM.Queue.RabbitMQ;
using MixFRM.Queue.RabbitMQ.Types;
using RabbitMQ.Client.Events;
using System;

namespace Test.RabbitMQ.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            QueueOptions queueOptions = new QueueOptions()
            {
                QueueName = "test2"
            };


            for (int i = 0; i < 1000; i++)
            {
                string message = "Mail" + i;
                RabbitMQPublishManager.BasicPublish(message, queueOptions, null);
            }


        }
         
    }
}

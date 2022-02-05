using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Queue.RabbitMQ.Types
{
    public class PublishOptions
    {
        public string Exchange { get; set; }
        public IBasicProperties BasicProperties { get; set; }
    }
}

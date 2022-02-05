using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Queue.RabbitMQ.Types
{
    public class QueueOptions
    {
        /// <summary>
        /// Kuyruk Adı
        /// </summary>
        public string QueueName { get; set; }
        /// <summary>
        /// Kuyruğun yaşam süresini bilgisini içerir. Default olarak false 'tur. False değerde kuyruk in-memory olarak oluşturulur. False durumda broker restart olduğununda queue silinir. Eğer persistence olmasını isteniyorsa true set edilmelidir. 
        /// </summary>
        public bool Durable { get; set; } = false;
        /// <summary>
        /// Kuyruğun başka connectionlar ile birlikte kullanılıp kullanılmayacağı bilgisini içerir.
        /// </summary>
        public bool Exclusive { get; set; } = false;
        /// <summary>
        /// Kuyruğa gönderilen veri consumer tarafına geçmesi ile birlikte kuyruğun silinmesi ile ilgili bilgiyi içerir. Default olarak false 'tur. False olduğu durumda consumer tarafında mesaj işlendiğinde Acknowledge bilgisi gönderilmelidir. Eğer değer true olursa, consumer mesajı alır almaz işlendi bilgisi gönderir.
        /// </summary>
        public bool AutoDelete { get; set; } = false;
        public Dictionary<string,object> Arguments { get; set; }
    }
}

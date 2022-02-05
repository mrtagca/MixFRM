using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Queue.RabbitMQ
{
    public static class RabbitMQConfigurations
    {
        static RabbitMQConfigurations()
        {
            IConfiguration appSetting = new ConfigurationBuilder()
                    .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
            IConfigurationSection section = appSetting.GetSection("QueueConfiguration");
            HostName = section.GetSection("HostName").Value;
            UserName = section.GetSection("UserName").Value;
            Password = section.GetSection("Password").Value;
        }

        public static string HostName { get; private set; }
        public static string UserName { get; private set; }
        public static string Password { get; private set; }

    }
}

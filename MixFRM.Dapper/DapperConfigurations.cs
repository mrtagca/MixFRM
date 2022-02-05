using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Dapper
{
    public static class DapperConfigurations
    {
        static DapperConfigurations()
        {
            IConfiguration appSetting = new ConfigurationBuilder()
                     .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                     .AddJsonFile("appsettings.json")
                     .Build();
            IConfigurationSection section = appSetting.GetSection("DapperConfiguration");
            ConnectionString = section.GetSection("ConnectionString").Value;
        }

        public static string ConnectionString { get; private set; }
    }
}

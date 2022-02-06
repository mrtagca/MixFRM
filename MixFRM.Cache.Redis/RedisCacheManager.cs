using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using MixFRM.Interfaces.Cache;
using MixFRM.Utils.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace MixFRM.Cache.Redis
{
    public class RedisCacheManager : ICacheManager
    {
        private IDatabase _database;
        private RedisCacheOptions options;
        private string defaultConnectionString { get; set; }
        private static ConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheManager()
        {
            IConfiguration appSetting = new ConfigurationBuilder()
                     .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                     .AddJsonFile("appsettings.json")
                     .Build();
            IConfigurationSection section = appSetting.GetSection("CacheConfiguration");

            defaultConnectionString = section.GetSection("CacheConnectionString:Redis").Value;

            options = new RedisCacheOptions()
            {
                Configuration = defaultConnectionString
            };
            _connectionMultiplexer = ConnectionMultiplexer.Connect(defaultConnectionString);
            _database = _connectionMultiplexer.GetDatabase(int.Parse(section.GetSection("CacheConnectionString:Database").Value));
        }

        public T Get<T>(string cacheKey)
        {
            try
            {
                var value = Encoding.UTF8.GetString(_database.StringGet(cacheKey));

                if (!string.IsNullOrEmpty(value))
                {
                    var valueObject = FrmJsonSerializer.Deserialize<T>(value);
                    return valueObject;
                }

                return default(T);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public bool Set<T>(string cacheKey, T model)
        {
            try
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(90)
                };

                var redisCache = new RedisCache(options);
                var valueString = FrmJsonSerializer.Serialize(model);
                return _database.StringSet(cacheKey, valueString);
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool Remove(object key)
        {
            return _database.KeyDelete((string)key);
        }

        public bool Clear()
        {
            try
            {
                var endpoints = _connectionMultiplexer.GetEndPoints(true);
                foreach (var endpoint in endpoints)
                {
                    var server = _connectionMultiplexer.GetServer(endpoint);
                    server.FlushAllDatabases();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool Contains(object key)
        {
            return _database.KeyExists((string)key);

        }

    }
}

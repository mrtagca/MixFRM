using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
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
        private string cacheProvider = ConfigurationManager.AppSettings.Get("CacheProvider");
        private string defaultConnectionString { get; set; }
        private static ConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheManager()
        {
            defaultConnectionString = ConfigurationManager.AppSettings.Get("CacheConnectionString:Redis");
            options = new RedisCacheOptions()
            {
                Configuration = defaultConnectionString
            };
            _connectionMultiplexer = ConnectionMultiplexer.Connect(defaultConnectionString);
        }

        public T Get<T>(string cacheKey)
        {
            using (var redisCache = new RedisCache(options))
            {
                var value = Encoding.UTF8.GetString(redisCache.Get(cacheKey));

                if (!string.IsNullOrEmpty(value))
                {
                    var valueObject = FrmJsonSerializer.Deserialize<T>(value);
                    return valueObject;
                }

                return default(T);
            }
        }

        public void Set<T>(string cacheKey, T model)
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(90)
            };

            using (var redisCache = new RedisCache(options))
            {
                var valueString = FrmJsonSerializer.Serialize(model);
                redisCache.SetString(cacheKey, valueString);
            }

        }

        public void Remove(object key)
        {
            using (var redisCache = new RedisCache(options))
            {
                redisCache.Remove((string)key);
            }
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
            return _database.KeyExists((RedisKey)key);

        }

    }
}

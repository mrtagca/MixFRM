using MixFRM.Cache.Redis;
using System;

namespace Test.Cache.Redis
{
    class Program
    {
        public Program()
        {

        }
        static void Main(string[] args)
        {
            RedisCacheManager redisCacheManager = new RedisCacheManager();

            string name = "test";
            bool isSet = redisCacheManager.Set<string>("filozof", name);
            string prs = redisCacheManager.Get<string>("filozof");
            bool containsKey = redisCacheManager.Contains("filozof"); // => not real contains, run the equals command

            //redisCacheManager.Remove("filozof");
            //redisCacheManager.Clear(); //flush all cache servers

        }
    }
}

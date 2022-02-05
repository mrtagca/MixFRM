using MixFRM.Interfaces.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Cache.Redis
{
    public interface IRedisService<T> where T : class, ICacheManager<T>
    {
    }
}

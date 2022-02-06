using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Interfaces.Cache
{
    public interface ICacheManager
    {
        bool Set<T>(string cacheKey, T Model);
        bool Clear();
        T Get<T>(string cacheKey);
        bool Contains(object cacheKey);
        bool Remove(object cacheKey);

    }
}

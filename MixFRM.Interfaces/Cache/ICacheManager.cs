using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Interfaces.Cache
{
    public interface ICacheManager
    {
        void Set<T>(string cacheKey, T Model);
        bool Clear();
        T Get<T>(string cacheKey);
        bool Contains(object cacheKey);

        void Remove(object cacheKey);

    }
}

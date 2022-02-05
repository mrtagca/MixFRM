using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Interfaces.Cache
{
    public interface ICacheManager<T> where T : class
    {
        List<T> GetAll(string cacheKey);
        T GetById(string cacheKey);

    }
}

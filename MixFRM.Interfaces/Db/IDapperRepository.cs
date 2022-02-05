using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.Interfaces.Db
{
    public interface IDapperRepository<T> where T : class
    {
        T GetByIdAsync(int id);
        IReadOnlyList<T> GetAllAsync();
        int AddAsync(T entity);
        int UpdateAsync(T entity);
        int DeleteAsync(int id);
    }
}

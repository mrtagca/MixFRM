using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace MixFRM.Dapper
{
    public class RepositoryBase : IDisposable
    {
        public SqlTransaction DbTrans { get; private set; }

        public SqlTransaction ActiveTransaction()
        {
            if (DbTrans == null)
                DbTrans = Executer.CreateTransaction();
            return DbTrans;
        }

        public void CommitTransaction()
        {
            if (DbTrans != null)
                DbTrans.Commit();
        }

        public void RollbackTransaction()
        {
            if (DbTrans != null)
                DbTrans.Rollback();
        }

        public RepositoryBase(SqlTransaction dbTrans = null)
        {
            DbTrans = dbTrans;
        }

        public RepositoryBase(bool createTransaction)
        {
            if (createTransaction)
                DbTrans = Executer.CreateTransaction();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}

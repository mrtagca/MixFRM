using Dapper;
using MixFRM.BaseTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MixFRM.Dapper
{
    public static class Executer
    {
        private static MethodInfo _gridReader_Read_Method;
        private static MethodInfo _gridReader_ReadFirstOrDefault_Method;
        static Executer()
        {
            _gridReader_Read_Method = typeof(SqlMapper.GridReader).GetMethods().FirstOrDefault(x => x.Name == "Read" && x.IsGenericMethod);
            _gridReader_ReadFirstOrDefault_Method = typeof(SqlMapper.GridReader).GetMethods().FirstOrDefault(x => x.Name == "ReadFirstOrDefault" && x.IsGenericMethod);
        }

        private static void RollbackTranAndCloseConnection(SqlConnection connection, SqlTransaction tran)
        {
            if (tran != null)
            {
                tran.Rollback();
                tran.Dispose();
            }
            if (connection != null)
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
                connection.Dispose();
            }
        }

        #region Transaction
        private static void ControlAndCloseConnection(SqlConnection connection, SqlTransaction tran)
        {
            if (tran == null) // SqlTransaction yoksa connection kapatılır. Varsa rollback ve ya commit sonrası connection kapatılır.
            {
                connection.Close();
                connection.Dispose();
            }
        }

        private static SqlConnection GetActiveConnection(SqlTransaction tran, string connectionString)
        {
            return tran != null && tran.Connection != null ? tran.Connection : new SqlConnection(connectionString ?? DapperConfigurations.ConnectionString);
        }

        public static SqlTransaction CreateTransaction(string connectionString = null)
        {
            SqlConnection cnn = new SqlConnection(connectionString ?? DapperConfigurations.ConnectionString);
            cnn.Open();
            return cnn.BeginTransaction();
        }
        #endregion

        #region Insert
        private static DbResultBase Insert(string connectionString, SqlTransaction tran, string query, object param)
        {
            DbResultBase result = new DbResultBase(tran);
            SqlConnection connection = GetActiveConnection(tran, connectionString);
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                result.RecordId = connection.QueryFirst<long>(query, param, tran);
                ControlAndCloseConnection(connection, tran);
            }
            catch (Exception exp)
            {
                RollbackTranAndCloseConnection(connection, tran);
                result.Error = new ErrorModel(exp);
            }
            return result;
        }

        public static DbResultBase Insert(string query, object param)
        {
            return Insert(DapperConfigurations.ConnectionString, null, query, param);
        }

        public static DbResultBase Insert(SqlTransaction tran, string query, object param)
        {
            return Insert(null, tran, query, param);
        }

        public static DbResultBase Insert(string connectionString, string query, object param)
        {
            return Insert(connectionString, null, query, param);
        }
        #endregion

        #region Execute
        private static DbResultBase Execute(string connectionString, SqlTransaction tran, string query, object param, CommandType? commandType = null)
        {
            DbResultBase result = new DbResultBase(tran);
            SqlConnection connection = GetActiveConnection(tran, connectionString);
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                result.AffectedRows = connection.Execute(query, param, tran, commandType: commandType);
                ControlAndCloseConnection(connection, tran);
            }
            catch (Exception exp)
            {
                RollbackTranAndCloseConnection(connection, tran);
                result.Error = new ErrorModel(exp);
            }
            return result;
        }

        public static DbResultBase Execute(string query, object param, CommandType? commandType = null)
        {
            return Execute(DapperConfigurations.ConnectionString, null, query, param, commandType);
        }

        public static DbResultBase Execute(SqlTransaction tran, string query, object param, CommandType? commandType = null)
        {
            return Execute(null, tran, query, param, commandType);
        }

        public static DbResultBase Execute(string connectionString, string query, object param, CommandType? commandType = null)
        {
            return Execute(connectionString, null, query, param, commandType);
        }
        #endregion


        #region Query
        private static DbResultList<T> Query<T>(string connectionString, SqlTransaction tran, string query, object param, CommandType? commandType = null)
        {
            DbResultList<T> result = new DbResultList<T>(tran);
            SqlConnection connection = GetActiveConnection(tran, connectionString);
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                result.Data = connection.Query<T>(query, param, tran, commandType: commandType).ToList();
                ControlAndCloseConnection(connection, tran);
            }
            catch (Exception exp)
            {
                RollbackTranAndCloseConnection(connection, tran);
                result.Error = new ErrorModel(exp);
            }
            return result;
        }

        public static DbResultList<T> Query<T>(string query, CommandType? commandType = null)
        {
            return Query<T>(DapperConfigurations.ConnectionString, null, query, null, commandType);
        }

        public static DbResultList<T> Query<T>(string query, object param, CommandType? commandType = null)
        {
            return Query<T>(DapperConfigurations.ConnectionString, null, query, param, commandType);
        }

        public static DbResultList<T> Query<T>(SqlTransaction tran, string query, object param, CommandType? commandType = null)
        {
            return Query<T>(null, tran, query, param, commandType);
        }

        public static DbResultList<T> Query<T>(string connectionString, string query, object param, CommandType? commandType = null)
        {
            return Query<T>(connectionString, null, query, param, commandType);
        }
        #endregion

        #region QueryMultiple
        private static ConcurrentDictionary<string, PropertyInfo[]> _readedTypePropeties = new ConcurrentDictionary<string, PropertyInfo[]>();
        private static PropertyInfo[] GetPropertyInfos(Type type)
        {
            if (_readedTypePropeties.TryGetValue(type.FullName, out PropertyInfo[] props))
                return props;
            props = type.GetProperties();
            _readedTypePropeties.TryAdd(type.FullName, props);
            return props;
        }

        private static DbResultData<T> QueryMultiple<T>(string connectionString, SqlTransaction tran, string query, object param)
        {
            DbResultData<T> result = new DbResultData<T>(tran);
            SqlConnection connection = GetActiveConnection(tran, connectionString);
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                SqlMapper.GridReader gridReader = connection.QueryMultiple(query, param, tran);
                result.Data = Activator.CreateInstance<T>();
                PropertyInfo[] properties = GetPropertyInfos(typeof(T));
                foreach (PropertyInfo property in properties)
                {
                    if (property.PropertyType.GenericTypeArguments.Length == 1)
                    {
                        MethodInfo generic = _gridReader_Read_Method.MakeGenericMethod(property.PropertyType.GenericTypeArguments[0]);
                        property.SetValue(result.Data, generic.Invoke(gridReader, new object[] { true }));
                    }
                    else
                    {
                        MethodInfo generic = _gridReader_ReadFirstOrDefault_Method.MakeGenericMethod(property.PropertyType);
                        property.SetValue(result.Data, generic.Invoke(gridReader, null));
                    }
                }
                ControlAndCloseConnection(connection, tran);
            }
            catch (Exception exp)
            {
                RollbackTranAndCloseConnection(connection, tran);
                result.Error = new ErrorModel(exp);
            }
            return result;
        }

        public static DbResultData<T> QueryMultiple<T>(string query, object param)
        {
            return QueryMultiple<T>(DapperConfigurations.ConnectionString, null, query, param);
        }

        public static DbResultData<T> QueryMultiple<T>(SqlTransaction tran, string query, object param)
        {
            return QueryMultiple<T>(null, tran, query, param);
        }

        public static DbResultData<T> QueryMultiple<T>(string connectionString, string query, object param)
        {
            return QueryMultiple<T>(connectionString, null, query, param);
        }
        #endregion

        #region First
        private static DbResultData<T> First<T>(string connectionString, SqlTransaction tran, string query, object param)
        {
            DbResultData<T> result = new DbResultData<T>(tran);
            SqlConnection connection = GetActiveConnection(tran, connectionString);
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                result.Data = connection.QueryFirst<T>(query, param, tran);
                ControlAndCloseConnection(connection, tran);
            }
            catch (Exception exp)
            {
                RollbackTranAndCloseConnection(connection, tran);
                result.Error = new ErrorModel(exp);
            }
            return result;
        }

        public static DbResultData<T> First<T>(string query, object param)
        {
            return First<T>(DapperConfigurations.ConnectionString, null, query, param);
        }

        public static DbResultData<T> First<T>(SqlTransaction tran, string query, object param)
        {
            return First<T>(null, tran, query, param);
        }

        public static DbResultData<T> First<T>(string connectionString, string query, object param)
        {
            return First<T>(connectionString, null, query, param);
        }
        #endregion

        #region FirstOrDefault
        private static DbResultData<T> FirstOrDefault<T>(string connectionString, SqlTransaction tran, string query, object param)
        {
            DbResultData<T> result = new DbResultData<T>(tran);
            SqlConnection connection = GetActiveConnection(tran, connectionString);
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                result.Data = connection.QueryFirstOrDefault<T>(query, param, tran);
                ControlAndCloseConnection(connection, tran);
            }
            catch (Exception exp)
            {
                RollbackTranAndCloseConnection(connection, tran);
                result.Error = new ErrorModel(exp);
            }
            return result;
        }

        public static DbResultData<T> FirstOrDefault<T>(string query, object param)
        {
            return FirstOrDefault<T>(DapperConfigurations.ConnectionString, null, query, param);
        }

        public static DbResultData<T> FirstOrDefault<T>(SqlTransaction tran, string query, object param)
        {
            return FirstOrDefault<T>(null, tran, query, param);
        }

        public static DbResultData<T> FirstOrDefault<T>(string connectionString, string query, object param)
        {
            return FirstOrDefault<T>(connectionString, null, query, param);
        }
        #endregion

        #region Single
        private static DbResultData<T> Single<T>(string connectionString, SqlTransaction tran, string query, object param)
        {
            DbResultData<T> result = new DbResultData<T>(tran);
            SqlConnection connection = GetActiveConnection(tran, connectionString);
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                result.Data = connection.QuerySingle<T>(query, param, tran);
                ControlAndCloseConnection(connection, tran);
            }
            catch (Exception exp)
            {
                RollbackTranAndCloseConnection(connection, tran);
                result.Error = new ErrorModel(exp);
            }
            return result;
        }

        public static DbResultData<T> Single<T>(string query, object param)
        {
            return Single<T>(DapperConfigurations.ConnectionString, null, query, param);
        }

        public static DbResultData<T> Single<T>(SqlTransaction tran, string query, object param)
        {
            return Single<T>(null, tran, query, param);
        }

        public static DbResultData<T> Single<T>(string connectionString, string query, object param)
        {
            return Single<T>(connectionString, null, query, param);
        }
        #endregion


        #region SingleOrDefault
        private static DbResultData<T> SingleOrDefault<T>(string connectionString, SqlTransaction tran, string query, object param)
        {
            DbResultData<T> result = new DbResultData<T>(tran);
            SqlConnection connection = GetActiveConnection(tran, connectionString);
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                result.Data = connection.QuerySingleOrDefault<T>(query, param, tran);
                connection.Close();
                connection.Dispose();
            }
            catch (Exception exp)
            {
                RollbackTranAndCloseConnection(connection, tran);
                result.Error = new ErrorModel(exp);
            }
            return result;
        }

        public static DbResultData<T> SingleOrDefault<T>(string query, object param)
        {
            return SingleOrDefault<T>(DapperConfigurations.ConnectionString, null, query, param);
        }

        public static DbResultData<T> SingleOrDefault<T>(SqlTransaction tran, string query, object param)
        {
            return SingleOrDefault<T>(null, tran, query, param);
        }

        public static DbResultData<T> SingleOrDefault<T>(string connectionString, string query, object param)
        {
            return SingleOrDefault<T>(connectionString, null, query, param);
        }
        #endregion

        #region ExtendedFunctions
        public static int Count(string query, object param)
        {
            return Count(DapperConfigurations.ConnectionString, query, param);
        }

        public static int Count(string connectionString, string query, object param)
        {
            int count = 0;
            SqlConnection connection = GetActiveConnection(null, connectionString);
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                count = connection.QueryFirst<int>("SELECT COUNT(*) FROM ( " + query + ") AS X", param);
                ControlAndCloseConnection(connection, null);
            }
            catch (Exception exp)
            {
                RollbackTranAndCloseConnection(connection, null);
                count = -1;
            }
            return count;
        }

        public static bool Exists(string query, object param)
        {
            return Count(DapperConfigurations.ConnectionString, query, param) > 0;
        }

        public static bool Exists(string connectionString, string query, object param)
        {
            return Count(connectionString, query, param) > 0;
        }
        #endregion
    }
}

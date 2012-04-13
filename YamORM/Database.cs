using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace YamORM
{
    public class Database : IDatabase
    {
        #region Fields
        private readonly IDbConnection _connection;
        private readonly IList<TableConfiguration> _tableConfigurations;
        private IDbTransaction _transaction;
        #endregion

        #region Constructors
        internal Database(IDbConnection connection, IList<TableConfiguration> tableConfigurations)
        {
            _connection = connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            _tableConfigurations = tableConfigurations;
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
            _connection.Dispose();
        }

        public void BeginTransaction()
        {
            _transaction = _connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_transaction == null)
                throw new Exception("Transaction is null. Cannot commit.");
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        public void RollbackTransaction()
        {
            if (_transaction == null)
                throw new Exception("Transaction is null. Cannot rollback.");
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public IList<TObject> Select<TObject>()
        {
            TableConfiguration tableConfiguration = getTableConfiguration(typeof(TObject));
            string[] columnNames = tableConfiguration.PropertyMaps.Select(x => x.ColumnName).ToArray();

            //TODO: How to handle different database dialects?
            string commandText = string.Format("SELECT {0} FROM {1};", string.Join(", ", columnNames), tableConfiguration.TableMap.TableName);

            QueryCommand<TObject> queryCommand = new QueryCommand<TObject>(_connection, _transaction, commandText, _tableConfigurations);
            
            foreach (PropertyMap propertyMap in tableConfiguration.PropertyMaps)
            {
                queryCommand.addMap(propertyMap.PropertyName, propertyMap.ColumnName);
            }

            return queryCommand.Execute();
        }

        public TObject Select<TObject>(object key)
        {
            TableConfiguration tableConfiguration = getTableConfiguration(typeof(TObject));
            PropertyMap keyPropertyMap = getKeyPropertyMap(tableConfiguration);
            string[] columnNames = tableConfiguration.PropertyMaps.Select(x => x.ColumnName).ToArray();

            //TODO: How to handle different database dialects?
            string commandText = string.Format("SELECT {0} FROM {1} WHERE {2} = {3}", string.Join(", ", columnNames), tableConfiguration.TableMap.TableName, keyPropertyMap.ColumnName, keyPropertyMap.ParameterName);

            QueryCommand<TObject> queryCommand = new QueryCommand<TObject>(_connection, _transaction, commandText, _tableConfigurations);
            
            object parameterValue = key;
            if (parameterValue == null)
                parameterValue = DBNull.Value;

            queryCommand.addParameter(keyPropertyMap.ParameterName, parameterValue, keyPropertyMap.ParameterType);

            foreach (PropertyMap propertyMap in tableConfiguration.PropertyMaps)
            {
                queryCommand.addMap(propertyMap.PropertyName, propertyMap.ColumnName);
            }

            return queryCommand.Execute().FirstOrDefault();
        }

        public void Insert(object obj)
        {
            //TODO: How to make Insert use NonQueryCommand and ScalarCommand?

            if (obj == null)
                throw new ArgumentNullException("obj");
            
            Type objectType = obj.GetType();
            TableConfiguration tableConfiguration = getTableConfiguration(objectType);
            PropertyMap keyPropertyMap = getKeyPropertyMap(tableConfiguration);
            IList<PropertyMap> propertyMaps = tableConfiguration.PropertyMaps.Where(x => x.KeyType != KeyType.Identity).ToList();
            bool keyIsIdentity = keyPropertyMap.KeyType == KeyType.Identity;

            string[] columnNames = propertyMaps.Select(x => x.ColumnName).ToArray();
            string[] parameterNames = propertyMaps.Select(x => x.ParameterName).ToArray();

            //TODO: How to handle different database dialects?
            string commandText = string.Format("INSERT INTO {0}({1}) VALUES({2})", tableConfiguration.TableMap.TableName, string.Join(", ", columnNames), string.Join(", ", parameterNames));
            if (keyIsIdentity)
                commandText = string.Format("{0}; SELECT SCOPE_IDENTITY()", commandText);

            using (IDbCommand command = _connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;

                if (_transaction != null)
                    command.Transaction = _transaction;

                addParameters(command, propertyMaps, obj);

                if (keyIsIdentity)
                {
                    object scalarValue = command.ExecuteScalar();
                    setPropertyValue(obj, keyPropertyMap.PropertyName, scalarValue);
                }
                else
                    command.ExecuteNonQuery();
            }
        }

        public void Update(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type objectType = obj.GetType();
            TableConfiguration tableConfiguration = getTableConfiguration(objectType);
            PropertyMap keyPropertyMap = getKeyPropertyMap(tableConfiguration);
            IList<PropertyMap> propertyMaps = tableConfiguration.PropertyMaps.Where(x => x.KeyType == KeyType.None).ToList();

            string[] setClauses = propertyMaps.Select(x => string.Format("{0} = {1}", x.ColumnName, x.ParameterName)).ToArray();

            //TODO: How to handle different database dialects?
            string commandText = string.Format("UPDATE {0} SET {1} WHERE {2} = {3}", tableConfiguration.TableMap.TableName, string.Join(", ", setClauses), keyPropertyMap.ColumnName, keyPropertyMap.ParameterName);

            NonQueryCommand nonQueryCommand = new NonQueryCommand(_connection, _transaction, commandText);
            
            foreach (PropertyMap propertyMap in tableConfiguration.PropertyMaps)
            {
                object parameterValue = getPropertyValue(obj, propertyMap.PropertyName);
                nonQueryCommand.addParameter(propertyMap.ParameterName, parameterValue, propertyMap.ParameterType);
            }

            nonQueryCommand.Execute();
        }

        public void Delete(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type objectType = obj.GetType();
            TableConfiguration tableConfiguration = getTableConfiguration(objectType);
            PropertyMap keyPropertyMap = getKeyPropertyMap(tableConfiguration);

            string commandText = string.Format("DELETE FROM {0} WHERE {1} = {2}", tableConfiguration.TableMap.TableName, keyPropertyMap.ColumnName, keyPropertyMap.ParameterName);

            NonQueryCommand nonQueryCommand = new NonQueryCommand(_connection, _transaction, commandText);

            object parameterValue = getPropertyValue(obj, keyPropertyMap.PropertyName);

            nonQueryCommand.addParameter(keyPropertyMap.ParameterName, parameterValue, keyPropertyMap.ParameterType);

            nonQueryCommand.Execute();
        }

        public INonQueryCommand NonQuery(string commandText, CommandType commandType = CommandType.Text)
        {
            return new NonQueryCommand(_connection, _transaction, commandText, commandType);
        }

        public IScalarCommand<TResult> Scalar<TResult>(string commandText, CommandType commandType = CommandType.Text)
        {
            return new ScalarCommand<TResult>(_connection, _transaction, commandText, commandType);
        }

        public IQueryCommand<TObject> Query<TObject>(string commandText, CommandType commandType = CommandType.Text)
        {
            return new QueryCommand<TObject>(_connection, _transaction, commandText, _tableConfigurations, commandType);
        }
        #endregion

        #region Private Helper Methods
        private void addParameters(IDbCommand command, IList<PropertyMap> propertyMaps, object obj)
        {
            foreach (PropertyMap propertyMap in propertyMaps)
            {
                object parameterValue = getPropertyValue(obj, propertyMap.PropertyName);

                addParameter(command, propertyMap.ParameterName, parameterValue, propertyMap.ParameterType);
            }
        }
        
        private void addParameter(IDbCommand command, string parameterName, object parameterValue, DbType? parameterType)
        {
            IDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;

            if (parameterType.HasValue)
                parameter.DbType = parameterType.Value;

            if (parameterValue == null)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = parameterValue;

            command.Parameters.Add(parameter);
        }

        private PropertyMap getKeyPropertyMap(TableConfiguration tableConfiguration)
        {
            PropertyMap result = tableConfiguration.PropertyMaps.Where(x => x.KeyType != KeyType.None).FirstOrDefault();
            if (result == null)
                throw new Exception(string.Format("Key property has not been configured for Type {0}", tableConfiguration.TableMap.ObjectType.FullName));

            return result;
        }

        private TableConfiguration getTableConfiguration(Type objectType)
        {
            TableConfiguration result = _tableConfigurations.Where(x => x.TableMap.ObjectType == objectType).FirstOrDefault();
            if (result == null)
                throw new Exception(string.Format("Type {0} has not been configured in the DatabaseFactory", objectType.FullName));
            
            return result;
        }

        private object getPropertyValue(object obj, string propertyName)
        {
            PropertyInfo prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanRead)
            {
                return prop.GetValue(obj, null);
            }
            return null;
        }
        
        private void setPropertyValue(object obj, string propertyName, object value)
        {
            PropertyInfo prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                object propertyValue = Convert.ChangeType(value, prop.PropertyType);
                prop.SetValue(obj, propertyValue, null);
            }
        }
        #endregion
    }
}

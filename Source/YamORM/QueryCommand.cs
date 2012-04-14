using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace YamORM
{
    public class QueryCommand<TObject> : CommandBase, IQueryCommand<TObject>
    {
        private IDictionary<string, string> _maps;
        private IList<TableConfiguration> _tableConfigurations;

        internal QueryCommand(IDbConnection connection, IDbTransaction transaction, string commandText, IList<TableConfiguration> tableConfigurations, CommandType commandType = CommandType.Text)
            : base(connection, transaction, commandText, commandType)
        {
            _maps = new Dictionary<string, string>();
            _tableConfigurations = tableConfigurations;
        }

        public QueryCommand<TObject> Parameter(string name, object value, DbType? dbType = null)
        {
            addParameter(name, value, dbType);
            return this;
        }

        public QueryCommand<TObject> Map<TProperty>(Expression<Func<TObject, TProperty>> propertyExpression, string columnName)
        {
            MemberExpression mExp = propertyExpression.Body as MemberExpression;
            if (mExp == null)
                throw new ArgumentException("Invalid Expression", "propertyExcepiton");

            addMap(mExp.Member.Name, columnName);

            return this;
        }

        public IList<TObject> Execute()
        {
            if (_maps.Count == 0)
            {
                Type objectType = typeof(TObject);
                TableConfiguration tableConfiguration = _tableConfigurations.Where(x => x.TableMap.ObjectType == objectType).FirstOrDefault();
                if (tableConfiguration == null)
                {
                    foreach (PropertyInfo propertyInfo in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        if (propertyInfo.CanRead && propertyInfo.CanWrite)
                            addMap(propertyInfo.Name, propertyInfo.Name);
                }
                else
                    tableConfiguration.PropertyMaps.ToList().ForEach(x => addMap(x.PropertyName, x.ColumnName));
            }

            IList<TObject> result = new List<TObject>();

            using (IDbCommand command = buildCommand())
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(populateObject<TObject>(reader));
                }
                reader.Close();
            }
            return result;
        }

        internal void addMap(string propertyName, string columnName)
        {
            if (_maps.ContainsKey(propertyName))
                _maps[propertyName] = columnName;
            else
                _maps.Add(propertyName, columnName);
        }

        private T populateObject<T>(IDataReader reader)
        {
            T result = Activator.CreateInstance<T>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                
                object columnValue = reader.GetValue(i);
                if (columnValue == DBNull.Value)
                    columnValue = null;

                string propertyName = columnName;
                if (_maps.Count > 0 && _maps.Keys.Contains(columnName))
                    propertyName = _maps[columnName];

                if (!string.IsNullOrWhiteSpace(propertyName))
                {
                    PropertyInfo prop = result.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(result, columnValue, null);
                    }
                }
            }
            return result;
        }
    }
}

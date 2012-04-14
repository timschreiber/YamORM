using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace YamORM
{
    public class TableConfigurator<TObject> : ITableConfigurator<TObject>
    {
        private readonly DatabaseFactory _factory;
        private Type _objectType;
        private string _tableName;
        private IList<PropertyMap> _propertyMaps;
        private IList<string> _ignores;
        private Regex _rxNonAlphanumeric;

        internal TableConfigurator(DatabaseFactory factory, string tableName)
        {
            _factory = factory;
            _objectType = typeof(TObject);
            _tableName = tableName;
            _propertyMaps = new List<PropertyMap>();
            _ignores = new List<string>();
            _rxNonAlphanumeric = new Regex(@"[^A-Z0-9]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public TableConfigurator<TObject> Key<TKey>(Expression<Func<TObject, TKey>> keyPropertyExpression, DatabaseGeneratedOption databaseGeneratedOption)
        {
            MemberExpression mExp = keyPropertyExpression.Body as MemberExpression;
            if (mExp != null)
            {
                _propertyMaps.Where(x => x.KeyType != KeyType.None).ToList().ForEach(x => x.KeyType = KeyType.None);
                string propertyName = mExp.Member.Name;

                Type propertyType = mExp.Member.ReflectedType;
                Debug.WriteLine(propertyType);

                KeyType keyType = databaseGeneratedOption == DatabaseGeneratedOption.Identity ? KeyType.Identity : KeyType.Key;

                addKey(propertyName, propertyType, keyType);
            }
            return this;
        }

        private void addKey(string propertyName, Type propertyType, KeyType keyType)
        {
            PropertyMap propertyMap = _propertyMaps.Where(x => x.PropertyName == propertyName).FirstOrDefault();
            if (propertyMap == null)
                _propertyMaps.Add(new PropertyMap { PropertyName = propertyName, PropertyType = propertyType, KeyType = keyType });
            else
                propertyMap.KeyType = keyType;
        }

        public TableConfigurator<TObject> Property<TProperty>(Expression<Func<TObject, TProperty>> propertyExpression, string columnName = null, DbType? columnType = null)
        {
            MemberExpression mExp = propertyExpression.Body as MemberExpression;
            if (mExp != null)
            {
                string propertyName = mExp.Member.Name;
                
                Type propertyType = mExp.Member.ReflectedType;
                Debug.WriteLine(propertyType);

                string colName = columnName ?? propertyName;
                string parameterName = string.Format("@{0}", colName);

                addPropertyMap(propertyName, propertyType, colName, parameterName, columnType);
            }
            return this;
        }

        private void addPropertyMap(string propertyName, Type propertyType, string columnName, string parameterName, DbType? columnType)
        {
            PropertyMap keyPropertyMap = _propertyMaps.Where(x => x.PropertyName == propertyName && x.KeyType != KeyType.None).FirstOrDefault();
            if (keyPropertyMap == null)
                _propertyMaps.Add(new PropertyMap { PropertyName = propertyName, PropertyType = propertyType, ColumnName = columnName, ParameterType = columnType, ParameterName = parameterName, KeyType = KeyType.None });
            else
            {
                keyPropertyMap.ColumnName = columnName;
                keyPropertyMap.ParameterType = columnType;
                keyPropertyMap.ParameterName = parameterName;
            }
        }

        public TableConfigurator<TObject> Ignore<TProperty>(Expression<Func<TObject, TProperty>> propertyExpression)
        {
            MemberExpression mExp = propertyExpression.Body as MemberExpression;
            if (mExp != null)
            {
                _ignores.Add(mExp.Member.Name);
            }
            return this;
        }

        public DatabaseFactory Configure()
        {
            Type objectType = typeof(TObject);
            if (_propertyMaps.Where(x => x.KeyType != KeyType.None).Count() == 0)
            {
                PropertyInfo[] propertyInfos = typeof(TObject).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    string propertyName = propertyInfo.Name;
                    Type propertyType = propertyInfo.PropertyType;
                    string testName1 = _rxNonAlphanumeric.Replace(propertyInfo.Name, string.Empty).ToLowerInvariant();
                    string testName2 = string.Format("{0}id", objectType.Name).ToLowerInvariant();
                    if (string.Compare(testName1, testName2, true) == 0 || string.Compare(testName1, "id", true) == 0)
                    {
                        addKey(propertyName, propertyType, KeyType.Key);
                    }
                }
            }

            if (_propertyMaps.Where(x => x.KeyType == KeyType.None).Count() == 0)
            {
                PropertyInfo[] propertyInfos = typeof(TObject).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    if (propertyInfo.CanRead && propertyInfo.CanWrite)
                    {
                        string propertyName = propertyInfo.Name;
                        Type propertyType = propertyInfo.PropertyType;
                        string parameterName = string.Format("@{0}", propertyName);
                        if (!_ignores.Contains(propertyInfo.Name))
                        {
                            addPropertyMap(propertyName, propertyType, propertyName, parameterName, null);
                        }
                    }
                }
            }

            TableConfiguration tc = _factory.TableConfigurations.Where(x => x.TableMap.ObjectType == _objectType).FirstOrDefault();
            if (tc == null)
                _factory.TableConfigurations.Add(new TableConfiguration
                {
                    TableMap = new TableMap
                    {
                        ObjectType = _objectType,
                        TableName = _tableName
                    },
                    PropertyMaps = _propertyMaps
                });
            else
            {
                tc.TableMap.TableName = _tableName;
                tc.PropertyMaps = _propertyMaps;
            }
            return _factory;
        }
    }
}

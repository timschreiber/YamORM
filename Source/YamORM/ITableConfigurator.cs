using System;
using System.Linq.Expressions;
using System.Data;

namespace YamORM
{
    public interface ITableConfigurator<TObject>
    {
        DatabaseFactory Configure();
        TableConfigurator<TObject> Key<TKey>(Expression<Func<TObject, TKey>> keyPropertyExpression, DatabaseGeneratedOption databaseGeneratedOption);
        TableConfigurator<TObject> Property<TProperty>(Expression<Func<TObject, TProperty>> propertyExpression, string columnName, DbType? columnType = null);
    }
}

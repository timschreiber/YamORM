using System;
using System.Linq.Expressions;
using System.Data;

namespace YamORM
{
    public interface IQueryCommand<TObject>
    {
        System.Collections.Generic.IList<TObject> Execute();
        QueryCommand<TObject> Map<TProperty>(Expression<Func<TObject, TProperty>> propertyExpression, string columnName);
        QueryCommand<TObject> Parameter(string name, object value, DbType? dbType = null);
    }
}

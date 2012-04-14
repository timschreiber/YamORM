using System;
using System.Data;

namespace YamORM
{
    public interface IScalarCommand<TResult>
    {
        TResult Execute();
        ScalarCommand<TResult> Parameter(string name, object value, DbType? dbType = null);
    }
}

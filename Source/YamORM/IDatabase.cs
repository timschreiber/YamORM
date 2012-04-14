using System;
using System.Collections.Generic;
using System.Data;

namespace YamORM
{
    public interface IDatabase : IDisposable
    {
        void BeginTransaction();
        void CommitTransaction();
        void Delete(object obj);
        void Insert(object obj);
        INonQueryCommand NonQuery(string commandText, CommandType commandType = CommandType.Text);
        IQueryCommand<TObject> Query<TObject>(string commandText, CommandType commandType = CommandType.Text);
        void RollbackTransaction();
        IScalarCommand<TResult> Scalar<TResult>(string commandText, CommandType commandType = CommandType.Text);
        IList<TObject> Select<TObject>();
        TObject Select<TObject>(object key);
        void Update(object obj);
    }
}

using System;
using System.Data;

namespace YamORM
{
    public interface INonQueryCommand
    {
        void Execute();
        NonQueryCommand Parameter(string name, object value, DbType? dbType = null);
    }
}

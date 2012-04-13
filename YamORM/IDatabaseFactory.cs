using System;

namespace YamORM
{
    public interface IDatabaseFactory
    {
        IDatabaseFactory Connection(string connectionString, string providerName);
        IDatabaseFactory Connection(string connectionStringName);
        IDatabase CreateDatabase();
        ITableConfigurator<TObject> Table<TObject>(string tableName = null);
    }
}

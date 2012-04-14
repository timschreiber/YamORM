using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace YamORM
{
    public sealed class DatabaseFactory : IDatabaseFactory
    {
        #region Constants
        const string DEFAULT_PROVIDER_NAME = "System.Data.SqlClient";
        #endregion

        #region Singleton
        private static readonly DatabaseFactory _instance = new DatabaseFactory();

        public static DatabaseFactory Instance
        {
            get { return _instance; }
        }
        private DatabaseFactory()
        {
            _tableConfigurations = new List<TableConfiguration>();
        }
        #endregion

        #region Fields
        private IDbConnection _connection;
        private IList<TableConfiguration> _tableConfigurations;
        #endregion

        #region Properties
        internal IList<TableConfiguration> TableConfigurations { get { return _tableConfigurations; } }
        #endregion

        #region Connection Methods
        public IDatabaseFactory Connection(string connectionString, string providerName)
        {
            if(string.IsNullOrWhiteSpace(providerName))
                providerName = DEFAULT_PROVIDER_NAME;

            DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);
            if (factory == null)
                throw new Exception(string.Format("Could not obtain factory for provider: {0}", providerName));

            IDbConnection connection = factory.CreateConnection();
            if (connection == null)
                throw new Exception("Could not obtain connection from factory.");

            connection.ConnectionString = connectionString;
            _connection = connection;

            return this;
        }

        public IDatabaseFactory Connection(string connectionStringName)
        {
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            string connectionString = connectionStringSettings.ConnectionString;
            string providerName = string.IsNullOrWhiteSpace(connectionStringSettings.ProviderName) ? null : connectionStringSettings.ProviderName;
            return Connection(connectionString, providerName);
        }
        #endregion

        #region Table Methods
        public ITableConfigurator<TObject> Table<TObject>(string tableName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                tableName = typeof(TObject).Name;

            return new TableConfigurator<TObject>(this, tableName);
        }
        #endregion

        #region Database Methods
        public IDatabase CreateDatabase()
        {
            return new Database(_connection, _tableConfigurations);
        }
        #endregion
    }
}

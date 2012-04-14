//-----------------------------------------------------------------------
// <copyright file="DatabaseFactory.cs">
//     Copyright (c) 2012 Timothy P. Schreiber
//     Permission is hereby granted, free of charge, to any person
//     obtaining a copy of this software and associated documentation
//     files (the "Software"), to deal in the Software without
//     restriction, including without limitation the rights to use, copy,
//     modify, merge, publish, distribute, sublicense, and/or sell copies
//     of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be
//     included in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//     EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//     NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//     HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//     WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//     DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace YamORM
{
    public sealed class DatabaseFactory : IDatabaseFactory
    {
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
        private string _providerName;
        #endregion

        #region Properties
        internal IList<TableConfiguration> TableConfigurations { get { return _tableConfigurations; } }
        #endregion

        #region Connection Methods
        public IDatabaseFactory Connection(string connectionString, string providerName)
        {
            _providerName = providerName ?? Constants.DEFAULT_PROVIDER_NAME;

            DbProviderFactory factory = DbProviderFactories.GetFactory(_providerName);
            if (factory == null)
                throw new Exception(string.Format("Could not obtain factory for provider: {0}", _providerName));

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
            return new Database(_connection, _tableConfigurations, _providerName);
        }
        #endregion
    }
}

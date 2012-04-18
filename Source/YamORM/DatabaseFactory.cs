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
        #region Fields
        private string _connectionString;
        private string _providerName;
        private IList<TableConfiguration> _tableConfigurations;
        #endregion

        #region Constructors
        public DatabaseFactory()
        {
            _tableConfigurations = new List<TableConfiguration>();
        }
        #endregion

        #region Properties
        internal IList<TableConfiguration> TableConfigurations
        {
            get { return _tableConfigurations; }
        }
        #endregion

        #region Connection Methods
        public IDatabaseFactory Connection(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _providerName = providerName ?? Constants.DEFAULT_PROVIDER_NAME;
            return this;
        }

        public IDatabaseFactory Connection(string connectionStringName)
        {
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            _connectionString = connectionStringSettings.ConnectionString;
            _providerName = string.IsNullOrWhiteSpace(connectionStringSettings.ProviderName) ? Constants.DEFAULT_PROVIDER_NAME : connectionStringSettings.ProviderName;
            return this;
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
            DbProviderFactory factory = DbProviderFactories.GetFactory(_providerName);
            if (factory == null)
                throw new Exception(string.Format("Could not obtain DbProviderFactory for provider: {0}", _providerName));

            IDbConnection connection = factory.CreateConnection();
            if (connection == null)
                throw new Exception("Could not obtain connection from factory.");

            connection.ConnectionString = _connectionString;

            return new Database(connection, _tableConfigurations, _providerName);
        }
        #endregion
    }
}

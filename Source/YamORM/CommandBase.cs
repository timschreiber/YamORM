//-----------------------------------------------------------------------
// <copyright file="CommandBase.cs">
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
using System.Data;
using System.Linq;

namespace YamORM
{
    public abstract class CommandBase
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly IList<Parameter> _parameters;
        private readonly string _commandText;
        private readonly CommandType _commandType;

        public CommandBase(IDbConnection connection, IDbTransaction transaction, string commandText, CommandType commandType)
        {
            _connection = connection;
            _transaction = transaction;
            _commandText = commandText;
            _commandType = commandType;
            _parameters = new List<Parameter>();
        }

        internal void addParameter(string name, object value, DbType? dbType)
        {
            object parameterValue = DBNull.Value;
            if (value != null)
                parameterValue = value;

            Parameter parameter = _parameters.Where(x => x.Name == name).FirstOrDefault();
            if(parameter == null)
                _parameters.Add(new Parameter { Name = name, DbType = dbType, Value = value } );
            else
            {
                parameter.Name = name;
                parameter.DbType = dbType;
                parameter.Value = value;
            }
        }

        protected IDbCommand buildCommand()
        {
            IDbCommand command = _connection.CreateCommand();

            command.CommandText = _commandText;
            command.CommandType = _commandType;

            if (_transaction != null)
                command.Transaction = _transaction;

            foreach (Parameter parameter in _parameters)
            {
                IDataParameter dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Name;
                
                if(parameter.DbType.HasValue)
                    dbParameter.DbType = parameter.DbType.Value;

                dbParameter.Value = parameter.Value;
                command.Parameters.Add(dbParameter);
            }

            return command;
        }
    }
}

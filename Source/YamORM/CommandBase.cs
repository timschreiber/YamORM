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

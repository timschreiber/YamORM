using System;
using System.Data;

namespace YamORM
{
    public class ScalarCommand<TResult> : CommandBase, IScalarCommand<TResult>
    {
        internal ScalarCommand(IDbConnection connection, IDbTransaction transaction, string commandText, CommandType commandType = CommandType.Text)
            : base(connection, transaction, commandText, commandType)
        {
        }

        public ScalarCommand<TResult> Parameter(string name, object value, DbType? dbType = null)
        {
            addParameter(name, value, dbType);
            return this;
        }

        public TResult Execute()
        {
            TResult result;

            using (IDbCommand command = buildCommand())
            {
                result = (TResult)Convert.ChangeType(command.ExecuteScalar(), typeof(TResult));
            }

            return result;
        }
    }
}

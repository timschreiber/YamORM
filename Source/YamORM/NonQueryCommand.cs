using System.Data;

namespace YamORM
{
    public class NonQueryCommand : CommandBase, INonQueryCommand
    {
        internal NonQueryCommand(IDbConnection connection, IDbTransaction transaction, string commandText, CommandType commandType = CommandType.Text)
            : base(connection, transaction, commandText, commandType)
        {
        }

        public NonQueryCommand Parameter(string name, object value, DbType? dbType = null)
        {
            addParameter(name, value, dbType);
            return this;
        }

        public void Execute()
        {
            using (IDbCommand command = buildCommand())
            {
                command.ExecuteNonQuery();
            }
        }
    }
}

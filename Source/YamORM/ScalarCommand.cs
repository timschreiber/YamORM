//-----------------------------------------------------------------------
// <copyright file="ScalarCommand.cs">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace YamORM
{
    internal class Parameter
    {
        public string Name { get; set; }
        public DbType? DbType { get; set; }
        public object Value { get; set; }
    }
}

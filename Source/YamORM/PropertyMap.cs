using System;
using System.Data;

namespace YamORM
{
    internal class PropertyMap
    {
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public string ColumnName { get; set; }
        public DbType? ParameterType { get; set; }
        public string ParameterName { get; set; }
        public KeyType KeyType { get; set; }
    }
}

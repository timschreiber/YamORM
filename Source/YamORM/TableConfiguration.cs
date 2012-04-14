using System.Collections.Generic;

namespace YamORM
{
    internal class TableConfiguration
    {
        public TableMap TableMap { get; set; }
        public IList<PropertyMap> PropertyMaps { get; set; }
    }
}

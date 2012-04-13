using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YamORM.TestConsole.Models
{
    public class Product
    {
        public string ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}

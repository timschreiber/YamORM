using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamORM;
using YamORM.TestConsole.Models;
using System.Data;

namespace YamORM.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            IDatabase data = DatabaseFactory.Instance
                .Connection("TestDb")
                .Table<Category>()
                    .Key(x => x.CategoryId, DatabaseGeneratedOption.Identity)
                    .Configure()
                .Table<Product>().Configure()
                .CreateDatabase();

            Category category1 = new Category { Name = "Category 1" };
            Category category2 = new Category { Name = "Category 2" };
            Product product1 = new Product { ProductId = "PROD123", Name = "Product 123", Description = "The first Product", Price = 19.99M };
            Product product2 = new Product { ProductId = "PROD234", Name = "Product 234", Description = "The second Product", Price = 24.99M };
            Product product3 = new Product { ProductId = "PROD345", Name = "Product 345", Description = "The third Product", Price = 29.99M };

            data.BeginTransaction();
            try
            {
                data.Insert(category1);
                data.Insert(category2);

                product1.CategoryId = category1.CategoryId;
                product2.CategoryId = category2.CategoryId;
                product3.CategoryId = category2.CategoryId;

                data.Insert(product1);
                data.Insert(product2);
                data.Insert(product3);

                data.CommitTransaction();
                
                Console.WriteLine("OK");
            }
            catch(Exception ex)
            {
                data.RollbackTransaction();
                Console.WriteLine(ex.Message);
            }

            IList<Product> products = data.Query<Product>("SELECT ProductId, CategoryId, Name, Description, Price FROM Product WHERE CategoryId = @CategoryId")
                .Parameter("@CategoryId", category2.CategoryId)
                .Execute();

            foreach (Product product in products)
            {
                Console.WriteLine(product.ProductId);
            }

            Console.WriteLine();

            Product prodX = data.Select<Product>("PROD123");
            Console.WriteLine("{0}\t{1}", prodX.ProductId, prodX.Name);

            Console.ReadKey();
        }
    }
}

//-----------------------------------------------------------------------
// <copyright file="Program.cs">
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
using YamORM.TestConsole.Models;

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

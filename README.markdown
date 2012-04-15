#Introducing YamORM
<img src="http://timschreiber.com/YamORM_240x160.png" align="right" alt="YamORM"/>

_"**Y**et **A**nother **M**icro-**ORM**."_

YamORM is a micro-ORM for .NET that simplifies mapping objects to and from SQL queries and provides shortcuts for common CRUD functionality, without all the bloat, oatmeal SQL, and performance issues of a full-scale ORM like Entity Framework or NHibernate. It was designed to be fast, simple and straightforward, to support multiple database providers, and to run flawlessly in partial trust environments.

##Features

* Supports Auto-mapping and also provides a Fluent interface for mapping your POCOs and properties to database tables and columns.
* Generates clean SQL for basic CRUD operations.
* Supports stored procedures and parameterized text queries.
* Supports transactions.
* Supports multiple databases (tested with SQL Server and MySQL).

##Code Samples

Consider the following tables for all code samples:

    CREATE TABLE [dbo].[Category](
        [CategoryId] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED 
    (
        [CategoryId] ASC
    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

    SET ANSI_NULLS ON
    GO
    SET QUOTED_IDENTIFIER ON
    GO
    CREATE TABLE [dbo].[Product](
        [ProductId] [nchar](10) NOT NULL,
        [CategoryId] [int] NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
        [Description] [nvarchar](1000) NOT NULL,
        [Price] [money] NOT NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED 
    (
        [ProductId] ASC
    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

    ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Product_Category] FOREIGN KEY([CategoryId])
    REFERENCES [dbo].[Category] ([CategoryId])
    GO
    ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK_Product_Category]
    GO

And consider the following classes for all code samples:

    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class Product
    {
        public string ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }

The following code samples show the basic usage of YamORM

####Configure DatabaseFactory with Fluent mapping
    
    IDatabaseFactory factory = DatabaseFactory.Instance
        .Connection("ConnectionStringName")
        .Table<Category>("Category")
            .Key(x => x.CategoryId, DatabaseGeneratedOption.Identity)
            .Property(x => x.CategoryId, "CategoryId")
            .Property(x => x.Name, "Name", DbType.String) // You can optionally specify the DbType
            .Configure()
        .Table<Product>("Product")
            .Key(x => x.ProductId, DatabaseGeneratedOption.None)
            .Property(x => x.ProductId, "ProductId")
            .Property(x => x.Name, "Name")
            .Property(x => x.Description, "Description")
            .Property(x => x.Price, "Price")
            .Configure();

####Configure DatabaseFactory with Auto-mapping

* If POCO type and database table names match, then YamORM will map them automatically.
* If POCO has a property that is just "Id" or the POCO type name concatenated with "Id," then that is assumed to be the Key property for the object. No database generation is assumed.
* If the property names are the same as the column names, then they will be mapped automatically.

With those rules in mind, the code required to configure the DatabaseFactory is quite a bit smaller:

    IDatabaseFactory factory = DatabaseFactory.Instance
        .Connection("ConnectionStringName")
        .Table<Category>()
            .Key(x => x.CategoryId, DatabaseGeneratedOption.Identity)
            .Configure()
        .Table<Product>()
        .Configure();

####Create Instance of YamORM Database

    using(IDatabase data = factory.CreateDatabase())
    {
        // Do stuff here...
    }

####Insert Records Using a Transaction

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

####Select All

    IList<Product> products = data.Select<Product>();
    
####Select By Key

    Product product = data.Select<Product>("PROD123");

####Update

    product.Price = 9.99M;
    data.Update(product);

####Delete

    data.Delete(product);

####Execute SQL and Map Result to a POCO List

    IList<Product> products = data.Query<Product>("SELECT ProductId, CategoryId, Name, Description, Price FROM Product WHERE CategoryId = @CategoryId")
        .Parameter("@CategoryId", 2)
        .Execute();

####Execute SQL That Returns a Scalar Result

    Category category3 = new Category { Name = "Category 3" };
    category3.CategoryId = data.Scalar<int>("INSERT INTO Category(Name) VALUES(@Name); SELECT SCOPE_IDENTITY();")
        .Parameter("@Name", category3.Name)
        .Execute();

####Execute SQL That Doesn't Return Anything

    data.NonQuery("DELETE FROM Product WHERE CategoryId < @MinCategoryId")
        .Parameter("@MinCategoryId", 2)
        .Execute();

##Licensing
YamORM is licensed under the MIT license.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
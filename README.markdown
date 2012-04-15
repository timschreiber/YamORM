#Introducing YamORM

_"**Y**et **A**nother **M**icro-**ORM**."_

YamORM is a micro-ORM for .NET that simplifies mapping objects to and from SQL queries and provides shortcuts for common CRUD functionality, without all the bloat, oatmeal SQL, and performance issues of a full-scale ORM like Entity Framework or NHibernate. It was designed to be fast, simple and straightforward, to support multiple database providers, and to run flawlessly in partial trust environments.

##Features

* Supports Auto-Mapping and also provides a Fluent interface for mapping your POCOs and properties to database tables and columns.
* Generates clean SQL for basic CRUD operations.
* Supports stored procedures and parameterized text queries.
* Supports transactions.
* Supports multiple databases (tested with SQL Server and MySQL).

## Code Samples

Consider the following tables and objects for all code samples:

    CREATE TABLE [dbo].[Category](
        [CategoryId] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED 
    (
        [CategoryId] ASC
    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    GO
    /****** Object:  Table [dbo].[Product]    Script Date: 04/14/2012 22:36:27 ******/
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
    /****** Object:  ForeignKey [FK_Product_Category]    Script Date: 04/14/2012 22:36:27 ******/
    ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Product_Category] FOREIGN KEY([CategoryId])
    REFERENCES [dbo].[Category] ([CategoryId])
    GO
    ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK_Product_Category]
    GO


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

### Create YamORM Database
Auto-maps Tables, Properties, and Keys. Auto-mapped Keys do not support Identities, so the Key for Category must be specified.

    IDatabase data = DatabaseFactory.Instance
        .Connection("TestDb")
        .Table<Category>()
            .Key(x => x.CategoryId, DatabaseGeneratedOption.Identity)
            .Configure()
        .Table<Product>().Configure()
        .CreateDatabase();

###Insert records using Transaction

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


###Retrieve a List of Objects using a SQL query
Retrieves Product records by CategoryId using auto-mapping of result fields to object properties

    IList<Product> products = data.Query<Product>("SELECT ProductId, CategoryId, Name, Description, Price FROM Product WHERE CategoryId = @CategoryId")
        .Parameter("@CategoryId", category2.CategoryId)
        .Execute();
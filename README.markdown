Full-scale ORMs like Entity Framework and NHibernate have become bloatware. It seems as though each subseqent version keeps trying to stuff in more functionality and abstract more data access from developers at the cost of complexity and performance. They may be appropriate for some large projects, but for others they may be overkill -- or killing performance. Sometimes it's better to go back to a simpler time, to get closer to the metal. Micro-ORMs fill that space. They do a subset of what an ORM does and do it fast.

YamORM is a Micro-ORM with a fluent interface for configuration and querying. It's a bit more formal and a little larger than other Micro-ORMs like Massive, Dapper, or PetaPoco; but it's becoming just as powerful and seems to be a little more friendly to IoC (at least as far as I've been able to tell). Its purpose is to balance ease of use with performance, and I think it's achieving that.

YamORM stands for "**Y**et **A**nother **M**icro-**ORM**."

##Licensing
[YamORM is licensed under the MIT license](/codeschreiber/YamORM/wiki/Licensing).

##Roadmap

* Support for returning dynamic objects (Expandos).
* Performance optimization
    * IL emission instead of reflection when populating objects from query results.
    * Caching
    * Creating CRUD commands during configuration instead of when they're called.

##Features

* Currently supports SQL Server and MySQL databases, with more to come.
* Fluent configuration interface for mapping objects to tables and properties to columns.
* Simple, clean SQL generation for basic CRUD functionality: SELECT all, SELECT by Id, INSERT, UPDATE, and DELETE.
* Support for Stored Procedures text queries, using a Fluent interface to:
    * Set parameter names and values, and
    * Map result columns to object properties
* Auto-mapping of Tables and Columns if they match Object and Property names.
* Auto-mapping of Keys if the Property and corresponding Column names match "Id" or "Id" appended to the Object name (underscores are ignored).
* Support for Transactions
* High performance - close to that of raw ADO.NET

## Code Samples

Database tables for code samples

    CREATE TABLE [dbo].[Category](
        [CategoryId] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED 
    (
        [CategoryId] ASC
    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    GO

    CREATE TABLE [dbo].[Product](
        [ProductId] [nvarchar](10) NOT NULL,
        [CategoryId] [int] NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [Description] [nvarchar](1000) NOT NULL,
        [Price] [money] NOT NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED 
    (
        [ProductId] ASC
    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    GO

    ALTER TABLE [dbo].[Product]  WITH NOCHECK ADD  CONSTRAINT [FK_Product_Category] FOREIGN KEY([CategoryId])
    REFERENCES [dbo].[Category] ([CategoryId])
    GO

    ALTER TABLE [dbo].[Product] NOCHECK CONSTRAINT [FK_Product_Category]
    GO

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
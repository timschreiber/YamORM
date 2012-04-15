#Introducing YamORM

_"**Y**et **A**nother **M**icro-**ORM**."_

YamORM is a micro-ORM for .NET that simplifies mapping objects to and from SQL queries and provides shortcuts for common CRUD functionality, without all the bloat, oatmeal SQL, and performance issues of a full-scale ORM like Entity Framework or NHibernate. It was designed to be fast, simple and straightforward, to support multiple database providers, and to run flawlessly in partial trust environments.

##Features

* Supports Auto-mapping and also provides a Fluent interface for mapping your POCOs and properties to database tables and columns.
* Generates clean SQL for basic CRUD operations.
* Supports stored procedures and parameterized text queries.
* Supports transactions.
* Supports multiple databases (tested with SQL Server and MySQL).

## Code Samples

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

And consider the following classes for all code samples.

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

#### Configure DatabaseFactory with Fluent mapping####
    
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

#### Configure DatabaseFactory with Auto-mapping####

* If POCO type and database table names match, then YamORM will map them automatically.
* If POCO has a property that is just "Id" or the POCO type name concatenated with "Id," then that is assumed to be the Key property for the object. No database generation is assumed.
* If the property names are the same as the column names, then they will be mapped automatically.

    IDatabaseFactory factory = DatabaseFactory.Instance
        .Connection("ConnectionStringName")
        .Table<Category>()
            .Key(x => x.CategoryId, DatabaseGeneratedOption.Identity)
            .Configure()
        .Table<Product>()
            .Configure();



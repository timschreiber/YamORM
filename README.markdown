#Introducing YamORM <img src="http://timschreiber.com/YamORM_240x160.png" align="right" width="240" height="160" border="0" alt="YamORM"/>

_"**Y**et **A**nother **M**icro-**ORM**."_

YamORM is a micro-ORM for .NET that simplifies mapping objects to and from SQL queries and provides shortcuts for common CRUD functionality, without all the bloat, oatmeal SQL, and performance issues of a full-scale ORM like Entity Framework or NHibernate. It was designed to be fast, simple and straightforward, to support multiple database providers, and to run flawlessly in partial trust environments.

##Features

* Supports Auto-mapping and also provides a Fluent interface for mapping your POCOs and properties to database tables and columns.
* Generates clean SQL for basic CRUD operations.
* Supports stored procedures and parameterized text queries.
* Supports transactions.
* Supports multiple databases (tested with SQL Server and MySQL).

##Code Samples

Find the tables and classes used in these code samples [here](/codeschreiber/YamORM/wiki/Tables-and-Classes-for-Code-Samples).

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
YamORM is licensed under the MIT license. More information [here](/codeschreiber/YamORM/wiki/Licensing).
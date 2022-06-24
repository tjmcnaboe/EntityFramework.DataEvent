# EntityFramework.DataEvent
Configure your services in program.cs

```csharp
builder.Services.AddDbContext<WebApplication1.WebAppDbContext>();
// add the scoped data event publisher
builder.Services.AddScoped<IDataEventPublisher,ServiceBusDataEventPublisher>();
// configure the publisher
builder.Services.Configure<ServiceBusDataEventPublisherOptions>(options=>
{
    options.TopicName = "dataevent";

});
```

setup your db context
```csharp
// create a class that implements IDataEventEntity  
//** any classes implementing this interface will be handled by DataEvents on EF CRUD operations
    public class Product : IDataEventEntity 
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } 
        public string Description { get; set; } 
        public double ? Price { get; set; }

        // you can use this type to identify the object type to be serialized by your event reciever eg service bus etc.
        public string GetDataType()
        {
            return "Products";
        }
    }

    public class Customer
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string Zipcode { get; set; }
    }


//  Create a data event database context 
    public class WebAppDbContext : DataEventDatabaseContext
    {
        // will publish events because type implements interface IDataEventEntity
        public DbSet<Product> Products { get; set; }
        //Will not publish events for customers because customers does not implement the interface
        public DbSet<Customer> Customers { get; set; }

        // inject your IDataEventPublisher and pass it to the base type
        public WebAppDbContext(IDataEventPublisher dataEventPublisher, IConfiguration config) :base(dataEventPublisher)
        {
        }
    }


    
    ```

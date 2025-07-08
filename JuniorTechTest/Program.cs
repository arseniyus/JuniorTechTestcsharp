using JuniorTechTest;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<OrdersDb>(opt => opt.UseInMemoryDatabase("OrdersList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/orders", async (OrdersDb db) =>
    await db.Orders.ToListAsync());

app.MapGet("/orders/{id}", async (string id, OrdersDb db) =>
// get the cost of each item in order and return total 
{
    var order = await db.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == id);
    if (order is null)
    {
        return Results.NotFound();
    }

    order.TotalCost = order.Items.Sum(i => i.Cost);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        OrderId = order.Id,
        CustomerId = order.CustomerId,
        ContainsAllergens = order.ContainsAllergens,
        Items = order.Items,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
        TotalCost = order.TotalCost
    });
    
    // need to return a new object to map results in memory
}); 
    
        
app.MapPost("/orders", async (Order order, OrdersDb db) =>
{
    // order ids must be 17 characters long
    // cust omer ids must be 6 characters long
    // if id.length == 16 && customerId.length == 6

    if (order.Id.Length == 17 && order.CustomerId.ToString().Length == 6)
    {
        order.CreatedAt = DateTime.Now;

        order.TotalCost = order.Items.Sum(i => i.Cost);

        db.Orders.Add(order);
        
        await db.SaveChangesAsync();

        return Results.Created($"/orders/{order.Id}", order);
    }

    return Results.NotFound();
    // orders posts should persist to the db with a timestamp
    // just made a new field in the model to accomodate that

});

app.MapPut("/orders/{id}", async (string Id, Order relevantOrder, OrdersDb db) =>
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == Id);
        if (order is null) return Results.NotFound();
        
        order.UpdatedAt = DateTime.Now;

        order.CustomerId = relevantOrder.CustomerId;
        order.ContainsAllergens = relevantOrder.ContainsAllergens;
        order.CreatedAt = relevantOrder.CreatedAt;

        order.Items.Clear();
        foreach (var item in relevantOrder.Items)
        {
            order.Items.Add(new OrderItem
            {
                Name = item.Name,
                Cost = item.Cost
            });
        }
        order.TotalCost = relevantOrder.TotalCost;

        await db.SaveChangesAsync();
        return Results.NoContent();

        // put in an extra field in the model to handle updates.
    });

app.MapGet("/orders/by-customer/{customerId}", async (int customerId, OrdersDb db) =>
 
{
    var allCustomerOrders = await db.Orders
        .Where(o => o.CustomerId == customerId)
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();

    if (allCustomerOrders is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(allCustomerOrders);
    
    // need to return a new object because of a new variable that isn't in the model
}); 

/* app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi(); */

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// The API requires a mechanism for persisting customer order data.

// For the purpose of this initial implementation and given the time constraint,
// an in-memory data store should be implemented to manage order data during the application's runtime.
// So Dependency Injection, Singleton DB, Singleton Pattern and Minimal API
// Define an Interface & concrete store
// Register a singleton in Program.cs
// Inject into controllers/services
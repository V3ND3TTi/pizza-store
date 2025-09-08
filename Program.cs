using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=pizzas.db"));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // http
    options.ListenLocalhost(5001, ListenOptions =>
    {
        ListenOptions.UseHttps(); // https with dev cert 
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

// Serve static files from wwwroot (Svelte build)
app.UseStaticFiles();

// SPA fallback → if no API route matches, serve index.html
app.MapFallbackToFile("index.html");

// API endpoints
app.MapGet("/pizzas", async (AppDbContext db) =>
    await db.Pizzas.ToListAsync());

app.MapGet("/pizzas/{id}", async (int id, AppDbContext db) =>
    await db.Pizzas.FindAsync(id) is Pizza pizza
        ? Results.Ok(pizza)
        : Results.NotFound());

app.MapPost("/pizzas", async (Pizza pizza, AppDbContext db) =>
{
    db.Pizzas.Add(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizzas/{pizza.Id}", pizza);
});

app.MapPut("/pizzas/{id}", async (int id, Pizza update, AppDbContext db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();

    pizza.Name = update.Name;
    pizza.Description = update.Description;
    await db.SaveChangesAsync();

    return Results.Ok(pizza); // return JSON, not 204
});

app.MapDelete("/pizzas/{id}", async (int id, AppDbContext db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();

    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();


// Pizza model + EF context
public class Pizza
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Pizza> Pizzas => Set<Pizza>();
}

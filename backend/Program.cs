using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Webstaurant.IDS.Api.Domains.EDI.Services;
using Webstaurant.IDS.Api.Domains.Inventory.Services;
using Webstaurant.IDS.Api.Domains.Orders.Services;
using Webstaurant.IDS.Api.Domains.Purchasing.Services;
using Webstaurant.IDS.Api.Infrastructure.Data;
using Webstaurant.IDS.Api.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Ensure URL listens on port 5067
builder.WebHost.UseUrls("http://localhost:5067");

// 1. Add Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

// 2. Configure CORS for React UI
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5174", "http://localhost:5067")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 3. Configure SQLite Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=ids.db");
});

// 4. Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// 5. Register Repositories
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IPurchasingRepository, PurchasingRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IEdiRepository, EdiRepository>();

// 6. Register Services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPurchasingService, PurchasingService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEdiService, EdiService>();

var app = builder.Build();

// Auto-seed Database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

// Serve frontend static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

// Auto-launch browser when the standalone executable is run
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        Console.WriteLine("\n=======================================================");
        Console.WriteLine(" WebstaurantStore Mini-IDS is running!");
        Console.WriteLine(" Access the ERP UI at: http://localhost:5067");
        Console.WriteLine("=======================================================\n");

        Process.Start(new ProcessStartInfo
        {
            FileName = "http://localhost:5067",
            UseShellExecute = true
        });
    }
    catch { }
});

app.Run();

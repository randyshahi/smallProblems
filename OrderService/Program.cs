using OrderService.Stores;
using OrderService.Services;
using OrderService.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<OrderStore>();
builder.Services.AddSingleton<IOrderStore, OrderStore>();
builder.Services.AddLogging();
builder.Services.AddSingleton<OrderService.Services.OrderService>();
builder.Services.AddSingleton<IOrderService, OrderService.Services.OrderService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();

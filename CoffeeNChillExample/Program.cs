using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

string connectionString = builder.Configuration["AzureWebJobsStorage"] ?? "UseDevelopmentStorage=true";

builder.Services.AddSingleton(await TableService.CreateAsync(connectionString, "MenuItems"));
builder.Services.AddSingleton(await BlobService.CreateAsync(connectionString, "staff-docs"));
builder.Services.AddSingleton(await QueueService.CreateAsync(connectionString, "order-processing-queue"));
builder.Services.AddSingleton(await OrderService.CreateAsync(connectionString, "Orders"));
builder.Services.AddSingleton(await FailedOrderService.CreateAsync(connectionString, "FailedOrders"));

var app = builder.Build();

app.Run();

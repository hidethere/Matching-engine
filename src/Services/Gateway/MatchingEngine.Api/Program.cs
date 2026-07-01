using MatchingEngine.Core;
using MatchingEngine.Gateway;
using MatchingEngine.Gateway.Dtos;
using MatchingEngine.Kafka;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<OrderGateway>();
var kafka = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
builder.Services.AddSingleton<IOrderLog>(_ => new KafkaOrderLog(kafka, "orders")); 

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Minimal API
app.MapPost("/api/v1/orders", (OrderRequest request, OrderGateway gateway, IOrderLog orderLog, HttpResponse response) =>
{
    var result = gateway.Accept(request);
    if (!result.Accepted)
        return Results.BadRequest(new OrderRejected(result.RejectReason!));

    if (result.IsNew)
        orderLog.Append(result.Order);
    else
        response.Headers["Idempotency-Replayed"] = "true";   // duplicate signal

    return Results.Accepted(
        $"/api/v1/orders/{result.Order.ClientOrderId}",
        new OrderResponse(result.Order.ClientOrderId));
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

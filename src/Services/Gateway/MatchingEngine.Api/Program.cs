using MatchingEngine.Core;
using MatchingEngine.Gateway;
using MatchingEngine.Gateway.Dtos;
using MatchingEngine.Kafka;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<OrderGateway>();
builder.Services.AddSingleton<IOrderLog>(_ => new KafkaOrderLog("localhost:9092", "orders")); 

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Minimal API
app.MapPost("/api/v1/orders", (OrderRequest request, OrderGateway gateway, IOrderLog orderLog) =>
{
    var result = gateway.Accept(request);
    if (!result.Accepted)
        return Results.BadRequest(
            new OrderResponse(
            result.Order,
            result.Accepted,
            result.RejectReason,
            result.IsNew
            )
        );

    if (result.IsNew)
        orderLog.Append(result.Order);

    return Results.Ok(
        new OrderResponse(
            result.Order,
            result.Accepted,
            result.RejectReason,
            result.IsNew
            )
        );

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

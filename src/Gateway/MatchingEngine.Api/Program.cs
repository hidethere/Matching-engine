using MatchingEngine.Api.services;
using MatchingEngine.Core;
using MatchingEngine.Gateway;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<OrderGateway>();
builder.Services.AddSingleton<IOrderLog, InMemoryOrderLog>();
builder.Services.AddSingleton(sp => new EngineHost("AAPL",
    sp.GetRequiredService<IOrderLog>(),
    sp.GetRequiredService<ILogger<EngineHost>>()));
builder.Services.AddHostedService<MatchingBackgroundService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Minimal API
app.MapPost("/api/v1/orders", (OrderRequest request, OrderGateway gateway, IOrderLog orderLog) =>
{
    var result = gateway.Accept(request);
    if (!result.Accepted)
        return Results.BadRequest(new { error = result.RejectReason });

    orderLog.Append(result.Order);
    return Results.Ok(new { orderId = result.Order.Id });

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

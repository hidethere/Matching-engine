using System.Net.WebSockets;
using System.Text;
using MatchingEngine.Contracts.Abstractions;
using MatchingEngine.Kafka;
using MatchingEngine.MarketData;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ITradeConsumer>(_ => new KafkaTradeConsumer("localhost:9092", "trades"));
builder.Services.AddSingleton<Ticker>();
builder.Services.AddSingleton<PriceBroadcaster>();

builder.Services.AddOpenApi();
var app = builder.Build();
app.UseWebSockets();

app.MapGet("/api/v1/marketdata/{symbol}", (string symbol, Ticker ticker) =>
{
    return ticker.Get(symbol) is long p ? Results.Ok(new { symbol, price = p })
                                        : Results.NotFound();
});

app.MapGet("/ws/v1/marketdata/{symbol}", async (string symbol, HttpContext ctx, Ticker ticker, PriceBroadcaster bus) =>
{
    if (!ctx.WebSockets.IsWebSocketRequest) { ctx.Response.StatusCode = 400; return; }
    using var socket = await ctx.WebSockets.AcceptWebSocketAsync();

    async Task Send(long price) =>
        await socket.SendAsync(Encoding.UTF8.GetBytes($"{{\"symbol\":\"{symbol}\",\"price\":{price}}}"),
                               WebSocketMessageType.Text, true, ctx.RequestAborted);
    if (ticker.Get(symbol) is long snapshot) await Send(snapshot);

    var (id, reader) = bus.Subscribe(symbol);
    try
    {
        await foreach (var price in reader.ReadAllAsync(ctx.RequestAborted))
            await Send(price);
    }
    catch (OperationCanceledException)
    {

    }
    finally
    {
        bus.Unsubscribe(id);
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.Run();

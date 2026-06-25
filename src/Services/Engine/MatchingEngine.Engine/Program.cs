using MatchingEngine.Core;
using MatchingEngine.Engine;
using MatchingEngine.Kafka;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IOrderLog>(_ => new KafkaOrderLog("localhost:9092", "orders"));
builder.Services.AddSingleton<ITradePublisher>(_ =>
    new KafkaTradePublisher("localhost:9092", "trades")
);
builder.Services.AddSingleton(sp => new EngineHost("AAPL",
    sp.GetRequiredService<IOrderLog>(),
    sp.GetRequiredService<ILogger<EngineHost>>()));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
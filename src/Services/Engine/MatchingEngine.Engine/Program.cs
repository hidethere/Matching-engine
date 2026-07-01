using MatchingEngine.Core;
using MatchingEngine.Engine;
using MatchingEngine.Kafka;

var builder = Host.CreateApplicationBuilder(args);
var kafka = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
builder.Services.AddSingleton<IOrderLog>(_ => new KafkaOrderLog(kafka, "orders"));
builder.Services.AddSingleton<ITradePublisher>(_ =>
    new KafkaTradePublisher(kafka, "trades")
);
builder.Services.AddSingleton(sp => new EngineHost(
    sp.GetRequiredService<IOrderLog>(),
    sp.GetRequiredService<ILogger<EngineHost>>()));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
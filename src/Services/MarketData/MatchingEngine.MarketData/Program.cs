using MatchingEngine.Contracts.Abstractions;
using MatchingEngine.Kafka;
using MatchingEngine.MarketData;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<ITradeConsumer>(_ => new KafkaTradeConsumer("localhost:9092", "trades"));
var host = builder.Build();
host.Run();

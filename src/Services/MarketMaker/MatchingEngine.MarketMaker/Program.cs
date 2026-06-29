using MatchingEngine.Core;
using MatchingEngine.Kafka;
using MatchingEngine.MarketMaker;

var builder = Host.CreateApplicationBuilder(args);
var feed = new BinanceFeed("BTCUSDT");

builder.Services.AddSingleton<IOrderLog>(_ => new KafkaOrderLog("localhost:9092", "orders"));
builder.Services.AddSingleton<IPriceFeed>(feed);
//builder.Services.AddSingleton<IPriceFeed>(_ => new FixedPrice("BTCUSDT", 6_000_000)); static value test only
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

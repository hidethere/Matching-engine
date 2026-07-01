using MatchingEngine.Core;
using MatchingEngine.Kafka;
using MatchingEngine.MarketMaker;
using MatchingEngine.MarketMaker.Feeds;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
var symbols = builder.Configuration.GetSection("Symbols").Get<string[]>() ?? ["BTCUSDT"];
var kafka = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
builder.Services.AddSingleton<IOrderLog>(_ => new KafkaOrderLog(kafka, "orders"));
builder.Services.AddSingleton<IPriceFeed>(_ => new BinanceFeed(symbols));
//builder.Services.AddSingleton<IPriceFeed>(_ => new FixedPrice("BTCUSDT", 6_000_000)); static value test only
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

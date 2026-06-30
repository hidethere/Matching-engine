using MatchingEngine.Core;
using MatchingEngine.Kafka;
using MatchingEngine.MarketMaker;
using MatchingEngine.MarketMaker.Feeds;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
var symbols = builder.Configuration.GetSection("Symbols").Get<string[]>() ?? ["BTCUSDT"];

builder.Services.AddSingleton<IOrderLog>(_ => new KafkaOrderLog("localhost:9092", "orders"));
builder.Services.AddSingleton<IPriceFeed>(_ => new BinanceFeed(symbols));
//builder.Services.AddSingleton<IPriceFeed>(_ => new FixedPrice("BTCUSDT", 6_000_000)); static value test only
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

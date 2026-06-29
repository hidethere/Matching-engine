using MatchingEngine.Core;
using MatchingEngine.FeedHandler;
using MatchingEngine.Kafka;

var builder = Host.CreateApplicationBuilder(args);
//var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "synthetic_AAPL_and_MSF_2012-06-21_message.csv");

//builder.Services.AddSingleton<IOrderFeed>(_ => new LobsterFeed(dataPath, "AAPL"));

builder.Services.AddSingleton<IOrderLog>(_ => new KafkaOrderLog("localhost:9092", "orders"));
builder.Services.AddHostedService<Worker>();
var host = builder.Build();

host.Run();

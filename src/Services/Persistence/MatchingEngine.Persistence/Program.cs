using MatchingEngine.Contracts.Abstractions;
using MatchingEngine.Kafka;
using MatchingEngine.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<ITradeConsumer>(_ => new KafkaTradeConsumer("localhost:9092", "trades", "persistence"));
var conn = builder.Configuration.GetConnectionString("Trades");
builder.Services.AddDbContextFactory<TradesDbContext>(options => options.UseNpgsql(conn));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
using (var db = host.Services.GetRequiredService<IDbContextFactory<TradesDbContext>>().CreateDbContext())
    db.Database.Migrate();

host.Run();

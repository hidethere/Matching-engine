using MatchingEngine.Contracts.Abstractions;
using MatchingEngine.Kafka;
using MatchingEngine.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
var kafka = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
builder.Services.AddSingleton<ITradeConsumer>(_ => new KafkaTradeConsumer(kafka, "trades", "persistence"));
var conn = builder.Configuration.GetConnectionString("Trades");
builder.Services.AddDbContextFactory<TradesDbContext>(options => options.UseNpgsql(conn));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
using (var db = host.Services.GetRequiredService<IDbContextFactory<TradesDbContext>>().CreateDbContext())
    db.Database.Migrate();

host.Run();

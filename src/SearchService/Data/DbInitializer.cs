using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using SearchService.Models;
using SearchService.Services;
using System.Text.Json;
namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            //Used retry as Mongo was not getting connected
            //var retryPolicy = Policy
            //    .Handle<MongoConnectionException>()
            //    .Or<TimeoutException>()
            //    .WaitAndRetryAsync(
            //    retryCount: 5,
            //    sleepDurationProvider: attempt => TimeSpan.FromSeconds(5),
            //    onRetry: (exception, sleepDuration, attempt, context) =>
            //    {
            //        Console.WriteLine($"Retry {attempt} of MongoDB connection after exception: {exception.Message}");
            //    });
            //await retryPolicy.ExecuteAsync(async () =>
            //{
            //    await DB.InitAsync("SearchDb", MongoClientSettings
            //        .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            //    await DB.Index<Item>()
            //        .Key(x => x.Make, KeyType.Text)
            //        .Key(x => x.Model, KeyType.Text)
            //        .Key(x => x.Color, KeyType.Text)
            //        .CreateAsync();
            //});

            await DB.InitAsync("SearchDb", MongoClientSettings
                    .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();
            var count = await DB.CountAsync<Item>();

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();

            var items = await httpClient.GetItemForSearchDb();

            Console.WriteLine(items.Count+" returned from auction service");

            if(items.Count > 0) { await DB.SaveAsync(items); }
            //if(count == 0)
            //{
            //    Console.WriteLine("No data - will attempt to seed");
            //    var itemData = await File.ReadAllTextAsync("Data/auctions.json");
            //    var options = new JsonSerializerOptions
            //    {
            //        PropertyNameCaseInsensitive = true,
            //    };
            //    var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);
            //    await DB.SaveAsync(items);
            //}
        }
    }
}

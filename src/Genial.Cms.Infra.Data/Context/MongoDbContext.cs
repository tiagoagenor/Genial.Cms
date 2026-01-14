using System;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(MongoDbConfiguration mongoDbConfiguration)
    {
        ArgumentNullException.ThrowIfNull(mongoDbConfiguration);
        ArgumentException.ThrowIfNullOrEmpty(mongoDbConfiguration.ConnectionString);
        ArgumentException.ThrowIfNullOrEmpty(mongoDbConfiguration.DatabaseName);

        var client = new MongoClient(mongoDbConfiguration.ConnectionString);
        _database = client.GetDatabase(mongoDbConfiguration.DatabaseName);
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
}


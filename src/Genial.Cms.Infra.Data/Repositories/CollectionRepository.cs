using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly IMongoCollection<Collection> _collection;
    private readonly MongoDbContext _mongoDbContext;

    public CollectionRepository(MongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
        _collection = mongoDbContext.GetCollection<Collection>("_collection");
    }

    public async Task<Collection> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        // Mantido para compatibilidade, mas agora usa Slug ao invés de Key
        return await _collection.Find(c => c.Slug == key).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Collection> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Collection> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.Name == name).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Collection> GetByNameAndStageIdAsync(string name, string stageId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.Name == name && c.StageId == stageId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Collection> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.Slug == slug).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Collection> GetBySlugAndStageIdAsync(string slug, string stageId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.Slug == slug && c.StageId == stageId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Collection> GetByCollectionNameAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.CollectionName == collectionName).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Collection> GetByCollectionNameAndStageIdAsync(string collectionName, string stageId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.CollectionName == collectionName && c.StageId == stageId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Collection>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).SortBy(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<List<Collection>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        // Mantido para compatibilidade, mas pode retornar todos já que não há mais campo Active
        return await _collection.Find(_ => true).SortBy(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<List<Collection>> GetByStageIdAsync(string stageId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(c => c.StageId == stageId).SortBy(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<(List<Collection> Collections, int Total)> GetByStageIdPaginatedAsync(string stageId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Collection>.Filter.Eq(c => c.StageId, stageId);

        // Contar total de documentos
        var total = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        // Aplicar paginação
        var skip = (page - 1) * pageSize;
        var collections = await _collection
            .Find(filter)
            .SortBy(c => c.CreatedAt)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (collections, total);
    }

    public async Task InsertAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(collection, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(c => c.Id == collection.Id, collection, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> CreateMongoCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar se a collection já existe
            var collections = await _mongoDbContext.Database.ListCollectionNamesAsync(cancellationToken: cancellationToken);
            var collectionList = await collections.ToListAsync(cancellationToken);
            var collectionExists = collectionList.Contains(collectionName);

            if (!collectionExists)
            {
                // Criar a collection vazia no MongoDB
                await _mongoDbContext.Database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
                return true;
            }

            return false; // Collection já existe
        }
        catch
        {
            return false;
        }
    }

    public async Task<BsonDocument?> GetCollectionItemByIdAsync(string collectionName, string itemId, CancellationToken cancellationToken = default)
    {
        var mongoCollection = _mongoDbContext.Database.GetCollection<BsonDocument>(collectionName);

        // Tentar parsear o ItemId como ObjectId
        if (ObjectId.TryParse(itemId, out var objectId))
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
            return await mongoCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            // Se não for ObjectId válido, tentar buscar como string
            var filter = Builders<BsonDocument>.Filter.Eq("_id", itemId);
            return await mongoCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }
    }

    public async Task<int> CountByStageIdAsync(string stageId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Collection>.Filter.Eq(c => c.StageId, stageId);
        return (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly IMongoCollection<Media> _collection;

    public MediaRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<Media>("_media");
    }

    public async Task<Media> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(m => m.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Media> GetByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(m => m.Url == url).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Media> GetByFileNameUrlAsync(string fileNameUrl, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(m => m.FileNameUrl == fileNameUrl).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Media>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true)
            .SortByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Media>> GetByTagsAsync(List<string> tags, CancellationToken cancellationToken = default)
    {
        if (tags == null || tags.Count == 0)
        {
            return new List<Media>();
        }

        var filter = Builders<Media>.Filter.AnyIn(m => m.Tags, tags);
        return await _collection.Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Media> Media, int Total)> GetPaginatedAsync(
        int page, 
        int pageSize, 
        List<string>? tags = null,
        string? contentType = null,
        string? extension = null,
        string? stageId = null,
        string sortBy = "createdAt",
        string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        // Construir filtros
        var filters = new List<FilterDefinition<Media>>();

        // Filtro por tags
        if (tags != null && tags.Count > 0)
        {
            filters.Add(Builders<Media>.Filter.AnyIn(m => m.Tags, tags));
        }

        // Filtro por contentType
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            filters.Add(Builders<Media>.Filter.Eq(m => m.ContentType, contentType));
        }

        // Filtro por extension
        if (!string.IsNullOrWhiteSpace(extension))
        {
            filters.Add(Builders<Media>.Filter.Eq(m => m.Extension, extension));
        }

        // Filtro por stageId (IMPORTANTE: sempre filtrar por stage do usuário)
        if (!string.IsNullOrWhiteSpace(stageId))
        {
            filters.Add(Builders<Media>.Filter.Eq(m => m.StageId, stageId));
        }

        // Combinar filtros
        var filter = filters.Count > 0
            ? Builders<Media>.Filter.And(filters)
            : FilterDefinition<Media>.Empty;

        // Contar total de documentos
        var total = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        // Construir ordenação
        SortDefinition<Media> sortDefinition;
        var isDescending = sortDirection?.ToLower() == "desc";

        switch (sortBy?.ToLower())
        {
            case "name":
                sortDefinition = isDescending
                    ? Builders<Media>.Sort.Descending(m => m.FileName)
                    : Builders<Media>.Sort.Ascending(m => m.FileName);
                break;
            case "filesize":
                sortDefinition = isDescending
                    ? Builders<Media>.Sort.Descending(m => m.FileSize)
                    : Builders<Media>.Sort.Ascending(m => m.FileSize);
                break;
            case "createdat":
            default:
                sortDefinition = isDescending
                    ? Builders<Media>.Sort.Descending(m => m.CreatedAt)
                    : Builders<Media>.Sort.Ascending(m => m.CreatedAt);
                break;
        }

        // Aplicar paginação
        var skip = (page - 1) * pageSize;
        var media = await _collection
            .Find(filter)
            .Sort(sortDefinition)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (media, total);
    }

    public async Task InsertAsync(Media media, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(media, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Media media, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(m => m.Id == media.Id, media, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<int> CountByStageIdAsync(string stageId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Media>.Filter.Eq(m => m.StageId, stageId);
        return (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<long> GetTotalFileSizeByStageIdAsync(string stageId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Media>.Filter.Eq(m => m.StageId, stageId);
        var mediaList = await _collection.Find(filter).ToListAsync(cancellationToken);
        return mediaList.Sum(m => m.FileSize);
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MongoDB.Driver;

namespace Genial.Cms.Infra.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<User>("_user");
    }

    public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).SortBy(u => u.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(User user, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(user, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(u => u.Id == id, cancellationToken);
    }
}


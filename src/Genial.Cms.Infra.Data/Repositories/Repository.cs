using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Genial.Cms.Infra.Data.Repositories;

public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IAggregateRoot
{
    protected readonly DbSet<TEntity> DbSet;

    protected Repository(ApplicationDbContext applicationDbContext)
    {
        DbSet = applicationDbContext.Set<TEntity>();
    }

    public void Add(TEntity obj)
    {
        DbSet.Add(obj);
    }
}

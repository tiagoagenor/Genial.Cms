using System.Threading.Tasks;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;

namespace Genial.Cms.Infra.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
	private readonly ApplicationDbContext _applicationDbContext;

	public UnitOfWork(ApplicationDbContext applicationDbContext)
	{
		_applicationDbContext = applicationDbContext;
	}

	public async Task<bool> CommitAsync()
	{
		return await _applicationDbContext.SaveEntitiesAsync();
	}

	public void Dispose()
	{
		_applicationDbContext.Dispose();
	}
}
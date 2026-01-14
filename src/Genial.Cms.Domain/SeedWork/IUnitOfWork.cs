using System.Threading.Tasks;

namespace Genial.Cms.Domain.SeedWork;

public interface IUnitOfWork
{
	Task<bool> CommitAsync();
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Queries;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.QueryHandlers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<GetUsersQueryResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetUsersQueryResult>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando todos os usuários");

        var users = await _userRepository.GetAllAsync(cancellationToken);

        _logger.LogInformation("Encontrados {Count} usuários", users.Count);

        return users.Select(u => new GetUsersQueryResult
        {
            Id = u.Id,
            Email = u.Email,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        });
    }
}

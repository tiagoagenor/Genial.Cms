using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.User;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.User;

public class GetUserCommandHandler : IRequestHandler<GetUserCommand, GetUserCommandResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMediator _bus;
    private readonly ILogger<GetUserCommandHandler> _logger;

    public GetUserCommandHandler(
        IUserRepository userRepository,
        IMediator bus,
        ILogger<GetUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<GetUserCommandResult> Handle(GetUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando usuário. Id: {UserId}", request.Id);

        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado. Id: {UserId}", request.Id);
            await _bus.Publish(new ExceptionNotification("021", "Usuário não encontrado.", ExceptionType.Client, "Id"), cancellationToken);
            return null;
        }

        _logger.LogInformation("Usuário encontrado. Id: {UserId}, Email: {Email}", user.Id, user.Email);

        return new GetUserCommandResult
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

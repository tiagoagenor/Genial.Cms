using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.User;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.User;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, DeleteUserCommandResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMediator _bus;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IMediator bus,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<DeleteUserCommandResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando exclusão de usuário. Id: {UserId}", request.Id);

        // Verificar se o usuário existe
        var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingUser == null)
        {
            _logger.LogWarning("Tentativa de excluir usuário inexistente. Id: {UserId}", request.Id);
            await _bus.Publish(new ExceptionNotification("020", "Usuário não encontrado.", ExceptionType.Client, "Id"), cancellationToken);
            return null;
        }

        // Excluir do MongoDB
        await _userRepository.DeleteAsync(request.Id, cancellationToken);

        _logger.LogInformation("Usuário excluído com sucesso. Id: {UserId}", request.Id);

        return new DeleteUserCommandResult
        {
            Success = true,
            Message = "Usuário excluído com sucesso."
        };
    }
}

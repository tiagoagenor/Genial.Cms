using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.User;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.User;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserCommandResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMediator _bus;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IMediator bus,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<UpdateUserCommandResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando atualização de usuário. Id: {UserId}", request.Id);

        // Verificar se o usuário existe
        var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingUser == null)
        {
            _logger.LogWarning("Tentativa de atualizar usuário inexistente. Id: {UserId}", request.Id);
            await _bus.Publish(new ExceptionNotification("018", "Usuário não encontrado.", ExceptionType.Client, "Id"), cancellationToken);
            return null;
        }

        // Verificar se o email já está em uso por outro usuário
        if (!string.IsNullOrEmpty(request.Email) && request.Email != existingUser.Email)
        {
            var userWithEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (userWithEmail != null && userWithEmail.Id != request.Id)
            {
                _logger.LogWarning("Tentativa de atualizar email para um já existente. Email: {Email}", request.Email);
                await _bus.Publish(new ExceptionNotification("019", "Este email já está cadastrado.", ExceptionType.Client, "Email"), cancellationToken);
                return null;
            }
        }

        // Atualizar dados do usuário
        if (!string.IsNullOrEmpty(request.Email))
        {
            existingUser.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.Password))
        {
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        existingUser.UpdatedAt = DateTime.UtcNow;

        // Atualizar no MongoDB
        await _userRepository.UpdateAsync(existingUser, cancellationToken);

        _logger.LogInformation("Usuário atualizado com sucesso. Id: {UserId}, Email: {Email}", existingUser.Id, existingUser.Email);

        return new UpdateUserCommandResult
        {
            Id = existingUser.Id,
            Email = existingUser.Email,
            UpdatedAt = existingUser.UpdatedAt
        };
    }
}

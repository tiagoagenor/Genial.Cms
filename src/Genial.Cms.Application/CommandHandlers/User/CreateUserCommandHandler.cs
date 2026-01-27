using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.User;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.User;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserCommandResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMediator _bus;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IMediator bus,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<CreateUserCommandResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando criação de usuário. Email: {Email}", request.Email);

        // Verificar se o email já existe
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            _logger.LogWarning("Tentativa de criar usuário com email já existente: {Email}", request.Email);
            await _bus.Publish(new ExceptionNotification("017", "Este email já está cadastrado.", ExceptionType.Client, "Email"), cancellationToken);
            return null;
        }

        // Criptografar a senha
        var passwordCriptografada = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Criar novo usuário
        var now = DateTime.UtcNow;
        var user = new Domain.Aggregates.User
        {
            Email = request.Email,
            Password = passwordCriptografada,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Inserir no MongoDB
        await _userRepository.InsertAsync(user, cancellationToken);

        _logger.LogInformation("Usuário criado com sucesso. Id: {UserId}, Email: {Email}", user.Id, user.Email);

        return new CreateUserCommandResult
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}

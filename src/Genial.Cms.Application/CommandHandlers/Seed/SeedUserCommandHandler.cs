using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Seed;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.Seed;

public class SeedUserCommandHandler : IRequestHandler<SeedUserCommand, SeedUserCommandResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SeedUserCommandHandler> _logger;

    public SeedUserCommandHandler(
        IUserRepository userRepository,
        ILogger<SeedUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<SeedUserCommandResult> Handle(SeedUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando seed de User");

        const string email = "admin@admin.com";
        const string password = "123456";

        // Verificar se o usuário já existe
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser != null)
        {
            _logger.LogInformation("Usuário já existe. Email: {Email}, Id: {Id}", email, existingUser.Id);
            return new SeedUserCommandResult
            {
                Success = true,
                Message = $"Usuário já existe. Email: {email}"
            };
        }

        // Criptografar a senha
        var passwordCriptografada = BCrypt.Net.BCrypt.HashPassword(password);

        // Criar novo usuário
        var now = DateTime.UtcNow;
        var user = new Domain.Aggregates.User
        {
            Email = email,
            Password = passwordCriptografada,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Inserir no MongoDB
        await _userRepository.InsertAsync(user, cancellationToken);
        _logger.LogInformation("Usuário criado com sucesso. Email: {Email}, Id: {Id}", email, user.Id);

        return new SeedUserCommandResult
        {
            Success = true,
            Message = $"Usuário criado com sucesso. Email: {email}"
        };
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Login;
using Genial.Cms.Application.Services;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IStageRepository _stageRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMediator _bus;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IStageRepository stageRepository,
        IJwtTokenService jwtTokenService,
        IMediator bus,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _stageRepository = stageRepository;
        _jwtTokenService = jwtTokenService;
        _bus = bus;
        _logger = logger;
    }

    public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando login para email: {Email}", request.Email);

        // Buscar usuário por email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Tentativa de login com email não encontrado: {Email}", request.Email);
            await _bus.Publish(new ExceptionNotification("006", "Email ou senha inválidos.", ExceptionType.Client, "Email"), cancellationToken);
            return null;
        }

        // Verificar senha
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Tentativa de login com senha inválida para email: {Email}", request.Email);
            await _bus.Publish(new ExceptionNotification("006", "Email ou senha inválidos.", ExceptionType.Client, "Password"), cancellationToken);
            return null;
        }

        // Buscar o primeiro stage ativo
        var stage = await _stageRepository.GetFirstAsync(cancellationToken);
        if (stage == null)
        {
            _logger.LogWarning("Nenhum stage encontrado no banco de dados. Login bloqueado para email: {Email}", request.Email);
            await _bus.Publish(new ExceptionNotification("007", "Nenhum stage configurado no sistema. Entre em contato com o administrador.", ExceptionType.Client), cancellationToken);
            return null;
        }

        // Gerar JWT
        var token = _jwtTokenService.GenerateToken(user.Id, user.Email, stage);

        _logger.LogInformation("Login realizado com sucesso. UserId: {UserId}, Email: {Email}", user.Id, user.Email);

        return new LoginCommandResult
        {
            Token = token
        };
    }
}


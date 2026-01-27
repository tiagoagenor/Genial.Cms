using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Stage;
using Genial.Cms.Application.Services;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.Stage;

public class ChangeStageCommandHandler : IRequestHandler<ChangeStageCommand, ChangeStageCommandResult>
{
    private readonly IStageRepository _stageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMediator _bus;
    private readonly ILogger<ChangeStageCommandHandler> _logger;

    public ChangeStageCommandHandler(
        IStageRepository stageRepository,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IMediator bus,
        ILogger<ChangeStageCommandHandler> logger)
    {
        _stageRepository = stageRepository;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _bus = bus;
        _logger = logger;
    }

    public async Task<ChangeStageCommandResult> Handle(ChangeStageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando troca de stage. StageId: {StageId}", request.StageId);

        // Verificar se o stage existe por ID
        var stage = await _stageRepository.GetByIdAsync(request.StageId, cancellationToken);
        if (stage == null)
        {
            _logger.LogWarning("Stage não encontrado. StageId: {StageId}", request.StageId);
            await _bus.Publish(new ExceptionNotification("008", "Stage não encontrado.", ExceptionType.Client, "StageId"), cancellationToken);
            return null;
        }

        if (!stage.Active)
        {
            _logger.LogWarning("Tentativa de trocar para stage inativo. StageId: {StageId}", request.StageId);
            await _bus.Publish(new ExceptionNotification("009", "Stage não está ativo.", ExceptionType.Client, "StageId"), cancellationToken);
            return null;
        }

        _logger.LogInformation("Stage encontrado. Key: {Key}, Label: {Label}", stage.Key, stage.Label);

        // Validar dados do usuário logado
        if (request.UserData == null || !request.UserData.IsValid())
        {
            _logger.LogWarning("UserData inválido ou não fornecido");
            await _bus.Publish(new ExceptionNotification("065", "Não foi possível identificar o usuário. Token inválido.", ExceptionType.Client, "UserData"), cancellationToken);
            return null!;
        }

        // Gerar novo JWT com o novo stage
        var token = _jwtTokenService.GenerateToken(request.UserData.UserId, request.UserData.Email, stage);

        _logger.LogInformation("Token gerado com sucesso para troca de stage. UserId: {UserId}, StageId: {StageId}", request.UserData.UserId, request.StageId);

        return new ChangeStageCommandResult
        {
            Token = token
        };
    }
}


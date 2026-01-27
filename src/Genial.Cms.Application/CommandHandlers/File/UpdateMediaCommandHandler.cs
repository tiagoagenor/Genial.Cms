#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.File;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.File;

public class UpdateMediaCommandHandler : IRequestHandler<UpdateMediaCommand, UpdateMediaCommandResult>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IMediator _bus;
    private readonly ILogger<UpdateMediaCommandHandler> _logger;

    public UpdateMediaCommandHandler(
        IMediaRepository mediaRepository,
        IMediator bus,
        ILogger<UpdateMediaCommandHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<UpdateMediaCommandResult> Handle(UpdateMediaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando media. Id: {Id}", request.Id);

        // Validar dados do usuário logado
        if (request.UserData == null || !request.UserData.IsValid())
        {
            _logger.LogWarning("UserData inválido ou não fornecido");
            await _bus.Publish(new ExceptionNotification("080", "Não foi possível identificar o usuário. Token inválido.", ExceptionType.Client, "UserData"), cancellationToken);
            return null!;
        }

        // Buscar media existente
        var media = await _mediaRepository.GetByIdAsync(request.Id, cancellationToken);
        if (media == null)
        {
            _logger.LogWarning("Media não encontrado. Id: {Id}", request.Id);
            await _bus.Publish(new ExceptionNotification("081", "Arquivo não encontrado", ExceptionType.Client, "Id"), cancellationToken);
            return null!;
        }

        // Verificar se o media pertence ao mesmo stage do usuário
        if (!string.IsNullOrWhiteSpace(media.StageId) && media.StageId != request.UserData.StageId)
        {
            _logger.LogWarning("Usuário tentando atualizar media de outro stage. MediaStageId: {MediaStageId}, UserStageId: {UserStageId}", 
                media.StageId, request.UserData.StageId);
            await _bus.Publish(new ExceptionNotification("082", "Você não tem permissão para atualizar este arquivo", ExceptionType.Client, "StageId"), cancellationToken);
            return null!;
        }

        // Atualizar apenas os campos permitidos
        if (request.Tags != null)
        {
            media.Tags = request.Tags;
        }

        // IMPORTANTE: Sempre manter/atualizar o StageId com o do usuário logado
        media.StageId = request.UserData.StageId;
        media.UpdatedAt = DateTime.UtcNow;

        // Salvar no MongoDB
        await _mediaRepository.UpdateAsync(media, cancellationToken);
        _logger.LogInformation("Media atualizado com sucesso. Id: {Id}, StageId: {StageId}", media.Id, media.StageId);

        return new UpdateMediaCommandResult
        {
            Id = media.Id,
            Tags = media.Tags,
            StageId = media.StageId,
            UpdatedAt = media.UpdatedAt
        };
    }
}

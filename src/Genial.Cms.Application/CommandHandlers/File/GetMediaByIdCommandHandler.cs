#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.File;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.File;

public class GetMediaByIdCommandHandler : IRequestHandler<GetMediaByIdCommand, GetMediaByIdCommandResult>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IMediator _bus;
    private readonly ILogger<GetMediaByIdCommandHandler> _logger;

    public GetMediaByIdCommandHandler(
        IMediaRepository mediaRepository,
        IMediator bus,
        ILogger<GetMediaByIdCommandHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<GetMediaByIdCommandResult> Handle(GetMediaByIdCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            await _bus.Publish(new ExceptionNotification("076", "Id é obrigatório", ExceptionType.Client, "Id"), cancellationToken);
            return null!;
        }

        var media = await _mediaRepository.GetByIdAsync(request.Id, cancellationToken);
        if (media == null)
        {
            _logger.LogWarning("Media não encontrado. Id: {Id}", request.Id);
            await _bus.Publish(new ExceptionNotification("074", "Arquivo não encontrado", ExceptionType.Client, "Id"), cancellationToken);
            return null!;
        }

        return new GetMediaByIdCommandResult
        {
            Data = new MediaByIdDto
            {
                Id = media.Id,
                OriginalFileName = media.FileName,
                FileNameUrl = media.FileNameUrl ?? string.Empty,
                ContentType = media.ContentType,
                FileSize = media.FileSize,
                Url = media.Url,
                Tags = media.Tags ?? new List<string>(),
                Extension = media.Extension,
                StageId = media.StageId,
                CreatedAt = media.CreatedAt,
                UpdatedAt = media.UpdatedAt
            }
        };
    }
}


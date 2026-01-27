using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.File;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.File;

public class GetMediaCommandHandler : IRequestHandler<GetMediaCommand, GetMediaCommandResult>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<GetMediaCommandHandler> _logger;

    public GetMediaCommandHandler(
        IMediaRepository mediaRepository,
        ILogger<GetMediaCommandHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    public async Task<GetMediaCommandResult> Handle(GetMediaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando media paginado. Page: {Page}, PageSize: {PageSize}, Tags: {Tags}, ContentType: {ContentType}, Extension: {Extension}, StageId: {StageId}, SortBy: {SortBy}, SortDirection: {SortDirection}",
            request.Page, request.PageSize, request.Tags != null ? string.Join(",", request.Tags) : "null", 
            request.ContentType ?? "null", request.Extension ?? "null", request.StageId ?? "null", request.SortBy, request.SortDirection);

        var (mediaList, total) = await _mediaRepository.GetPaginatedAsync(
            request.Page,
            request.PageSize,
            request.Tags,
            request.ContentType,
            request.Extension,
            request.StageId, // Filtrar por stage do usuário
            request.SortBy,
            request.SortDirection,
            cancellationToken);

        _logger.LogInformation("Encontrados {Count} arquivos (Total: {Total}) no stage {StageId}",
            mediaList.Count, total, request.StageId ?? "null");

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        var result = new GetMediaCommandResult
        {
            Data = mediaList.Select(m => new MediaDto
            {
                Id = m.Id,
                FileName = m.FileName,
                FileNameUrl = m.FileNameUrl ?? string.Empty,
                ContentType = m.ContentType,
                FileSize = m.FileSize,
                Url = m.Url,
                Tags = m.Tags ?? new List<string>(),
                Extension = m.Extension,
                StageId = m.StageId,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            }).ToList(),
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return result;
    }
}

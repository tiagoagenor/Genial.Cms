using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class GetCollectionsCommandHandler : IRequestHandler<GetCollectionsCommand, GetCollectionsCommandResult>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILogger<GetCollectionsCommandHandler> _logger;

    public GetCollectionsCommandHandler(
        ICollectionRepository collectionRepository,
        ILogger<GetCollectionsCommandHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _logger = logger;
    }

    public async Task<GetCollectionsCommandResult> Handle(GetCollectionsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando collections para o stage: {StageId}, Page: {Page}, PageSize: {PageSize}",
            request.StageId, request.Page, request.PageSize);

        var (collections, total) = await _collectionRepository.GetByStageIdPaginatedAsync(
            request.StageId,
            request.Page,
            request.PageSize,
            cancellationToken);

        _logger.LogInformation("Encontradas {Count} collections para o stage {StageId} (Total: {Total})",
            collections.Count, request.StageId, total);

        var totalPages = (int)System.Math.Ceiling(total / (double)request.PageSize);

        var result = new GetCollectionsCommandResult
        {
            Data = collections.Select(c => new CollectionNameDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList(),
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return result;
    }
}

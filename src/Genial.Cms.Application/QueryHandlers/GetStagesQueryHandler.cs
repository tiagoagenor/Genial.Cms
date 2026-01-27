using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Queries;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.QueryHandlers;

public class GetStagesQueryHandler : IRequestHandler<GetStagesQuery, IEnumerable<Stage>>
{
    private readonly IStageRepository _stageRepository;
    private readonly ILogger<GetStagesQueryHandler> _logger;

    public GetStagesQueryHandler(
        IStageRepository stageRepository,
        ILogger<GetStagesQueryHandler> logger)
    {
        _stageRepository = stageRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Stage>> Handle(GetStagesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando todos os stages");

        var stages = await _stageRepository.GetAllAsync(cancellationToken);

        _logger.LogInformation("Encontrados {Count} stages", stages.Count);

        return stages;
    }
}


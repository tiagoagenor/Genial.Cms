using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Queries;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.QueryHandlers;

public class GetStatisticsByStageQueryHandler : IRequestHandler<GetStatisticsByStageQuery, IEnumerable<GetStatisticsByStageQueryResult>>
{
    private readonly IStageRepository _stageRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<GetStatisticsByStageQueryHandler> _logger;

    public GetStatisticsByStageQueryHandler(
        IStageRepository stageRepository,
        IUserRepository userRepository,
        ICollectionRepository collectionRepository,
        IMediaRepository mediaRepository,
        ILogger<GetStatisticsByStageQueryHandler> logger)
    {
        _stageRepository = stageRepository;
        _userRepository = userRepository;
        _collectionRepository = collectionRepository;
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetStatisticsByStageQueryResult>> Handle(GetStatisticsByStageQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando estatísticas por stage");

        // Buscar todos os stages
        var stages = await _stageRepository.GetAllAsync(cancellationToken);

        // Buscar total de usuários (geral, já que não há relação direta com stage)
        var allUsers = await _userRepository.GetAllAsync(cancellationToken);
        var totalUsers = allUsers.Count;

        // Criar lista de resultados
        var results = new List<GetStatisticsByStageQueryResult>();

        // Para cada stage, buscar estatísticas
        foreach (var stage in stages)
        {
            var totalCollections = await _collectionRepository.CountByStageIdAsync(stage.Id, cancellationToken);
            var totalMedia = await _mediaRepository.CountByStageIdAsync(stage.Id, cancellationToken);
            var totalMediaFileSize = await _mediaRepository.GetTotalFileSizeByStageIdAsync(stage.Id, cancellationToken);

            results.Add(new GetStatisticsByStageQueryResult
            {
                StageId = stage.Id,
                StageKey = stage.Key,
                StageLabel = stage.Label,
                TotalUsers = totalUsers, // Total geral de usuários para todos os stages
                TotalCollections = totalCollections,
                TotalMedia = totalMedia,
                TotalMediaFileSize = totalMediaFileSize,
                TotalMediaFileSizeFormatted = FormatFileSize(totalMediaFileSize)
            });
        }

        _logger.LogInformation("Estatísticas calculadas para {Count} stages", results.Count);

        return results;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        // Formatar com 2 casas decimais, mas remover zeros desnecessários
        return $"{len:0.##} {sizes[order]}";
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Seed;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.Seed;

public class SeedStagesCommandHandler : IRequestHandler<SeedStagesCommand, SeedStagesCommandResult>
{
    private readonly IStageRepository _stageRepository;
    private readonly ILogger<SeedStagesCommandHandler> _logger;

    public SeedStagesCommandHandler(
        IStageRepository stageRepository,
        ILogger<SeedStagesCommandHandler> logger)
    {
        _stageRepository = stageRepository;
        _logger = logger;
    }

    public async Task<SeedStagesCommandResult> Handle(SeedStagesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando seed de Stages");

        var stagesToSeed = new List<Domain.Aggregates.Stage>
        {
            new Domain.Aggregates.Stage
            {
                Key = "dev",
                Label = "Dev",
                Letter = "D",
                Description = "Development environment",
                Order = 1,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Stage
            {
                Key = "hml",
                Label = "HML",
                Letter = "H",
                Description = "Homologation environment",
                Order = 2,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Stage
            {
                Key = "prod",
                Label = "Prod",
                Letter = "P",
                Description = "Production environment",
                Order = 3,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var createdCount = 0;
        var existingCount = 0;

        foreach (var stage in stagesToSeed)
        {
            // Verificar se o stage já existe pela key
            var existingStage = await _stageRepository.GetByKeyAsync(stage.Key, cancellationToken);
            if (existingStage != null)
            {
                _logger.LogInformation("Stage já existe. Key: {Key}, Id: {Id}", stage.Key, existingStage.Id);
                existingCount++;
                continue;
            }

            // Inserir stage no MongoDB
            await _stageRepository.InsertAsync(stage, cancellationToken);
            _logger.LogInformation("Stage criado com sucesso. Key: {Key}, Id: {Id}", stage.Key, stage.Id);
            createdCount++;
        }

        var message = $"Stages: {createdCount} criado(s), {existingCount} já existente(s).";

        _logger.LogInformation("Seed de Stages concluído. {Message}", message);

        return new SeedStagesCommandResult
        {
            Success = true,
            Message = message
        };
    }
}

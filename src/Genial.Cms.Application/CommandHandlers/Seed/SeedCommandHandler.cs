using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Seed;
using Genial.Cms.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.Seed;

public class SeedCommandHandler : IRequestHandler<SeedCommand, SeedCommandResult>
{
    private readonly IMediator _bus;
    private readonly ILogger<SeedCommandHandler> _logger;

    public SeedCommandHandler(
        IMediator bus,
        ILogger<SeedCommandHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task<SeedCommandResult> Handle(SeedCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando seed do banco de dados");

        var messages = new List<string>();

        // Seed de Stages
        _logger.LogInformation("Executando seed de Stages");
        var seedStagesResult = await _bus.Send(new SeedStagesCommand(), cancellationToken);
        if (seedStagesResult != null && seedStagesResult.Success)
        {
            messages.Add(seedStagesResult.Message);
        }

        // Seed de Fields
        _logger.LogInformation("Executando seed de Fields");
        var seedFieldsResult = await _bus.Send(new SeedFieldsCommand(), cancellationToken);
        if (seedFieldsResult != null && seedFieldsResult.Success)
        {
            messages.Add(seedFieldsResult.Message);
        }

        // Seed de User
        _logger.LogInformation("Executando seed de User");
        var seedUserResult = await _bus.Send(new SeedUserCommand(), cancellationToken);
        if (seedUserResult != null && seedUserResult.Success)
        {
            messages.Add(seedUserResult.Message);
        }

        // Seed de TypeFiles
        _logger.LogInformation("Executando seed de TypeFiles");
        var seedTypeFilesResult = await _bus.Send(new SeedTypeFilesCommand(), cancellationToken);
        if (seedTypeFilesResult != null && seedTypeFilesResult.Success)
        {
            messages.Add(seedTypeFilesResult.Message);
        }

        var finalMessage = string.Join(" | ", messages);

        return new SeedCommandResult
        {
            Success = true,
            Message = finalMessage
        };
    }
}


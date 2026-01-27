using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Seed;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.Seed;

public class SeedFieldsCommandHandler : IRequestHandler<SeedFieldsCommand, SeedFieldsCommandResult>
{
    private readonly IFieldRepository _fieldRepository;
    private readonly ILogger<SeedFieldsCommandHandler> _logger;

    public SeedFieldsCommandHandler(
        IFieldRepository fieldRepository,
        ILogger<SeedFieldsCommandHandler> logger)
    {
        _fieldRepository = fieldRepository;
        _logger = logger;
    }

    public async Task<SeedFieldsCommandResult> Handle(SeedFieldsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando seed de Fields");

        var fieldsToSeed = new List<Domain.Aggregates.Field>
        {
            new Domain.Aggregates.Field
            {
                Key = "input",
                Label = "Input",
                Type = "input",
                Icon = "fas fa-font",
                Active = true,
                Order = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Field
            {
                Key = "text",
                Label = "Texto",
                Type = "text",
                Icon = "fas fa-font",
                Active = true,
                Order = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Field
            {
                Key = "number",
                Label = "Number",
                Type = "number",
                Icon = "fas fa-font",
                Active = true,
                Order = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Field
            {
                Key = "email",
                Label = "Email",
                Type = "email",
                Icon = "fas fa-font",
                Active = true,
                Order = 4,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Field
            {
                Key = "select",
                Label = "Select",
                Type = "select",
                Icon = "fas fa-font",
                Active = true,
                Order = 5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Field
            {
                Key = "radio",
                Label = "Radio",
                Type = "radio",
                Icon = "fas fa-font",
                Active = true,
                Order = 6,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Field
            {
                Key = "bool",
                Label = "Bool",
                Type = "bool",
                Icon = "fas fa-font",
                Active = true,
                Order = 7,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Field
            {
                Key = "checkbox",
                Label = "CheckBox",
                Type = "checkbox",
                Icon = "fas fa-font",
                Active = true,
                Order = 8,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Domain.Aggregates.Field
            {
                Key = "file",
                Label = "File",
                Type = "file",
                Icon = "fas fa-font",
                Active = true,
                Order = 9,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var createdCount = 0;
        var existingCount = 0;

        foreach (var field in fieldsToSeed)
        {
            // Verificar se o field já existe pela key
            var existingField = await _fieldRepository.GetByKeyAsync(field.Key, cancellationToken);
            if (existingField != null)
            {
                _logger.LogInformation("Field já existe. Key: {Key}, Id: {Id}", field.Key, existingField.Id);
                existingCount++;
                continue;
            }

            // Inserir field no MongoDB
            await _fieldRepository.InsertAsync(field, cancellationToken);
            _logger.LogInformation("Field criado com sucesso. Key: {Key}, Id: {Id}", field.Key, field.Id);
            createdCount++;
        }

        var message = $"Fields: {createdCount} criado(s), {existingCount} já existente(s).";

        _logger.LogInformation("Seed de Fields concluído. {Message}", message);

        return new SeedFieldsCommandResult
        {
            Success = true,
            Message = message
        };
    }
}


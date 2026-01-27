using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Field;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.Field;

public class CreateFieldCommandHandler : IRequestHandler<CreateFieldCommand, CreateFieldCommandResult>
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IMediator _bus;
    private readonly ILogger<CreateFieldCommandHandler> _logger;

    public CreateFieldCommandHandler(
        IFieldRepository fieldRepository,
        IMediator bus,
        ILogger<CreateFieldCommandHandler> logger)
    {
        _fieldRepository = fieldRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<CreateFieldCommandResult> Handle(CreateFieldCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando criação de field. Key: {Key}", request.Key);

        // Verificar se o field já existe pela key
        var existingField = await _fieldRepository.GetByKeyAsync(request.Key, cancellationToken);
        if (existingField != null)
        {
            _logger.LogWarning("Tentativa de criar field com key já existente: {Key}", request.Key);
            await _bus.Publish(new ExceptionNotification("016", "Já existe um field com esta chave.", ExceptionType.Client, "Key"), cancellationToken);
            return null;
        }

        // Criar novo field
        var now = DateTime.UtcNow;
        var field = new Domain.Aggregates.Field
        {
            Key = request.Key,
            Label = request.Label,
            Type = request.Type.ToLower(),
            Active = request.Active,
            Order = request.Order,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Inserir no MongoDB
        await _fieldRepository.InsertAsync(field, cancellationToken);

        _logger.LogInformation("Field criado com sucesso. Id: {FieldId}, Key: {Key}", field.Id, field.Key);

        return new CreateFieldCommandResult
        {
            Id = field.Id,
            Key = field.Key,
            Label = field.Label,
            Type = field.Type,
            Active = field.Active,
            Order = field.Order
        };
    }
}


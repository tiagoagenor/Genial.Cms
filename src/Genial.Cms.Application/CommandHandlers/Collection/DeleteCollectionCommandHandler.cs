#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class DeleteCollectionCommandHandler : IRequestHandler<DeleteCollectionCommand, DeleteCollectionCommandResult>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly MongoDbContext _mongoDbContext;
    private readonly IMediator _bus;
    private readonly ILogger<DeleteCollectionCommandHandler> _logger;

    public DeleteCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        MongoDbContext mongoDbContext,
        IMediator bus,
        ILogger<DeleteCollectionCommandHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _mongoDbContext = mongoDbContext;
        _bus = bus;
        _logger = logger;
    }

    public async Task<DeleteCollectionCommandResult> Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando deleção de collection '{CollectionId}'", request.CollectionId);

        // Validar CollectionId
        if (string.IsNullOrWhiteSpace(request.CollectionId))
        {
            _logger.LogWarning("CollectionId não foi fornecido");
            await _bus.Publish(new ExceptionNotification("064", "CollectionId é obrigatório", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        // Buscar a collection existente
        var collection = await _collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken);
        if (collection == null)
        {
            _logger.LogWarning("Collection não encontrada: {CollectionId}", request.CollectionId);
            await _bus.Publish(new ExceptionNotification("061", "Collection não encontrada", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        // Deletar a collection MongoDB dinâmica se existir
        if (!string.IsNullOrWhiteSpace(collection.CollectionName))
        {
            try
            {
                await _mongoDbContext.Database.DropCollectionAsync(collection.CollectionName, cancellationToken);
                _logger.LogInformation("Collection MongoDB '{CollectionName}' deletada com sucesso", collection.CollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao deletar collection MongoDB '{CollectionName}'. Continuando com a deleção da collection principal.", collection.CollectionName);
                // Continuar mesmo se houver erro ao deletar a collection MongoDB
            }
        }

        // Deletar a collection principal
        try
        {
            await _collectionRepository.DeleteAsync(request.CollectionId, cancellationToken);
            _logger.LogInformation("Collection '{CollectionId}' deletada com sucesso", request.CollectionId);

            return new DeleteCollectionCommandResult
            {
                Success = true,
                Message = "Collection deletada com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar collection. Exception: {Exception}", ex.ToString());
            await _bus.Publish(new ExceptionNotification("069", $"Erro ao deletar collection: {ex.Message}", ExceptionType.Server, "Collection"), cancellationToken);
            return null!;
        }
    }
}

#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class DeleteCollectionItemCommandHandler : IRequestHandler<DeleteCollectionItemCommand, DeleteCollectionItemResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly MongoDbContext _mongoDbContext;
    private readonly IMediator _bus;
    private readonly ILogger<DeleteCollectionItemCommandHandler> _logger;
    private readonly ICollectionItemChangeRepository _collectionItemChangeRepository;

    public DeleteCollectionItemCommandHandler(
        ICollectionRepository collectionRepository,
        MongoDbContext mongoDbContext,
        IMediator bus,
        ILogger<DeleteCollectionItemCommandHandler> logger,
        ICollectionItemChangeRepository collectionItemChangeRepository)
    {
        _collectionRepository = collectionRepository;
        _mongoDbContext = mongoDbContext;
        _bus = bus;
        _logger = logger;
        _collectionItemChangeRepository = collectionItemChangeRepository;
    }

    public async Task<DeleteCollectionItemResponse> Handle(DeleteCollectionItemCommand request, CancellationToken cancellationToken)
    {
        // Validar CollectionId e ItemId
        if (string.IsNullOrWhiteSpace(request.CollectionId))
        {
            _logger.LogWarning("CollectionId não foi fornecido");
            await _bus.Publish(new ExceptionNotification("064", "CollectionId é obrigatório", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        if (string.IsNullOrWhiteSpace(request.ItemId))
        {
            _logger.LogWarning("ItemId não foi fornecido");
            await _bus.Publish(new ExceptionNotification("066", "ItemId é obrigatório", ExceptionType.Client, "ItemId"), cancellationToken);
            return null!;
        }

        _logger.LogInformation("Deletando item {ItemId} da collection {CollectionId}", request.ItemId, request.CollectionId);

        // Buscar a collection
        var collection = await _collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken);
        if (collection == null)
        {
            _logger.LogWarning("Collection não encontrada: {CollectionId}", request.CollectionId);
            await _bus.Publish(new ExceptionNotification("061", "Collection não encontrada", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        if (string.IsNullOrWhiteSpace(collection.CollectionName))
        {
            _logger.LogError("Collection não possui CollectionName configurado: {CollectionId}", request.CollectionId);
            await _bus.Publish(new ExceptionNotification("062", "Collection não possui configuração de nome de collection MongoDB", ExceptionType.Server, "CollectionName"), cancellationToken);
            return null!;
        }

        // Buscar o item antes de deletar para salvar no histórico
        var mongoCollection = _mongoDbContext.Database.GetCollection<BsonDocument>(collection.CollectionName);
        var objectId = ObjectId.Parse(request.ItemId);
        var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
        var existingItem = await mongoCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (existingItem == null)
        {
            _logger.LogWarning("Item não encontrado para deletar: {ItemId} na collection {CollectionName}", request.ItemId, collection.CollectionName);
            await _bus.Publish(new ExceptionNotification("067", "Item não encontrado", ExceptionType.Client, "ItemId"), cancellationToken);
            return null!;
        }

        // Deletar o item
        var deleteResult = await mongoCollection.DeleteOneAsync(filter, cancellationToken);

        if (deleteResult.DeletedCount == 0)
        {
            _logger.LogWarning("Falha ao deletar item: {ItemId} na collection {CollectionName}", request.ItemId, collection.CollectionName);
            await _bus.Publish(new ExceptionNotification("067", "Item não encontrado", ExceptionType.Client, "ItemId"), cancellationToken);
            return null!;
        }

        _logger.LogInformation("Item deletado com sucesso: {ItemId} da collection {CollectionName}", request.ItemId, collection.CollectionName);

        // Salvar histórico de deleção (beforeData = documento deletado, afterData = null)
        if (request.UserData != null && !string.IsNullOrWhiteSpace(request.UserData.UserId))
        {
            try
            {
                var change = new CollectionItemChange
                {
                    CollectionId = request.CollectionId,
                    ItemId = request.ItemId,
                    UserId = request.UserData.UserId,
                    ChangeType = CollectionItemChangeTypeEnum.Delete.Id,
                    BeforeData = existingItem, // Dados antes da deleção
                    AfterData = null, // Deleção não tem dados posteriores
                    CreatedAt = DateTime.UtcNow
                };

                await _collectionItemChangeRepository.InsertAsync(change, cancellationToken);
                _logger.LogInformation("Histórico de deleção salvo para item {ItemId}", request.ItemId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao salvar histórico de deleção. Continuando...");
                // Não falhar a operação se houver erro ao salvar histórico
            }
        }

        return new DeleteCollectionItemResponse
        {
            Success = true,
            Message = "Item deletado com sucesso"
        };
    }
}

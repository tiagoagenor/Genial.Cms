#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.Collection;
using Genial.Cms.Domain.Aggregates;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Domain.SeedWork;
using CollectionAggregate = Genial.Cms.Domain.Aggregates.Collection;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Genial.Cms.Application.CommandHandlers.Collection;

public class UpdateCollectionCommandHandler : IRequestHandler<UpdateCollectionCommand, UpdateCollectionCommandResult>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IMediator _bus;
    private readonly ILogger<UpdateCollectionCommandHandler> _logger;

    public UpdateCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IMediator bus,
        ILogger<UpdateCollectionCommandHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<UpdateCollectionCommandResult> Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando atualização de collection '{CollectionId}'", request.CollectionId);

        // Validar CollectionId
        if (string.IsNullOrWhiteSpace(request.CollectionId))
        {
            _logger.LogWarning("CollectionId não foi fornecido");
            await _bus.Publish(new ExceptionNotification("064", "CollectionId é obrigatório", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        // Buscar a collection existente
        var existingCollection = await _collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken);
        if (existingCollection == null)
        {
            _logger.LogWarning("Collection não encontrada: {CollectionId}", request.CollectionId);
            await _bus.Publish(new ExceptionNotification("061", "Collection não encontrada", ExceptionType.Client, "CollectionId"), cancellationToken);
            return null!;
        }

        // Validar dados do usuário logado
        if (request.UserData == null || string.IsNullOrWhiteSpace(request.UserData.StageId))
        {
            _logger.LogWarning("StageId não encontrado no token do usuário");
            await _bus.Publish(new ExceptionNotification("065", "Não foi possível identificar o stage do usuário. Token inválido.", ExceptionType.Client, "StageId"), cancellationToken);
            return null!;
        }

        // Validar se a collection pertence ao stage do usuário
        if (existingCollection.StageId != request.UserData.StageId)
        {
            _logger.LogWarning("Collection não pertence ao stage do usuário. Collection StageId: {CollectionStageId}, User StageId: {UserStageId}", 
                existingCollection.StageId, request.UserData.StageId);
            await _bus.Publish(new ExceptionNotification("068", "Collection não pertence ao stage atual", ExceptionType.Client, "StageId"), cancellationToken);
            return null!;
        }

        // Validar Name
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Name não foi informado na requisição");
            await _bus.Publish(new ExceptionNotification("063", "O campo 'name' é obrigatório.", ExceptionType.Client, "Name"), cancellationToken);
            return null!;
        }

        // Validar se o name já existe no mesmo stage (mas não pode ser a própria collection)
        if (request.Name != existingCollection.Name)
        {
            var existingCollectionByNameAndStage = await _collectionRepository.GetByNameAndStageIdAsync(request.Name, request.UserData.StageId, cancellationToken);
            if (existingCollectionByNameAndStage != null && existingCollectionByNameAndStage.Id != request.CollectionId)
            {
                _logger.LogWarning("Nome já existe no mesmo stage. Name: {Name}, StageId: {StageId}", request.Name, request.UserData.StageId);
                await _bus.Publish(new ExceptionNotification("064", $"Já existe uma collection com o nome '{request.Name}' no mesmo stage.", ExceptionType.Client, "Name"), cancellationToken);
                return null!;
            }
        }

        if (request.Fields == null || request.Fields.Count == 0)
        {
            _logger.LogWarning("Nenhum field foi enviado na requisição");
            await _bus.Publish(new ExceptionNotification("058", "É necessário enviar pelo menos um campo.", ExceptionType.Client, "Fields"), cancellationToken);
            return null!;
        }

        // Processar cada campo (similar ao CreateCollectionCommandHandler)
        var collectionFields = new List<CollectionField>();
        var fieldResults = new List<CollectionFieldItemResultDto>();
        var nomesProcessados = new HashSet<string>();
        var now = DateTime.UtcNow;

        foreach (var field in request.Fields)
        {
            _logger.LogInformation("Processando field. Type: {Type}, Name: {Name}", field.Type, field.Name);

            // Validar se o nome já existe na requisição atual
            if (nomesProcessados.Contains(field.Name))
            {
                _logger.LogWarning("Nome duplicado encontrado na requisição: {Name}", field.Name);
                await _bus.Publish(new ExceptionNotification("061", $"Já existe um campo com o nome '{field.Name}'. Nomes devem ser únicos.", ExceptionType.Client, "Name"), cancellationToken);
                return null!;
            }

            nomesProcessados.Add(field.Name);

            // Selecionar apenas a Data correspondente ao Type
            object data = field.Type.ToLower() switch
            {
                "file" => field.FileData ?? new FileDataDto(),
                "input" => field.InputData ?? new InputDataDto(),
                "text" => field.TextData ?? new TextDataDto(),
                "number" => field.NumberData ?? new NumberDataDto(),
                "email" => field.EmailData ?? new EmailDataDto(),
                "select" => field.SelectData ?? new SelectDataDto(),
                "radio" => field.RadioData ?? new RadioDataDto(),
                "bool" => field.BoolData ?? new BoolDataDto { Required = false },
                "checkbox" => field.CheckboxData ?? new CheckboxDataDto(),
                "range" => field.RangeData ?? new RangeDataDto(),
                "color" => field.ColorData ?? new ColorDataDto { Required = false },
                _ => new object()
            };

            // Serializar apenas a Data correta para BsonDocument
            BsonDocument bsonDocument;
            try
            {
                var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
                });
                bsonDocument = BsonDocument.Parse(jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao serializar data para BsonDocument. Type: {Type}", field.Type);
                await _bus.Publish(new ExceptionNotification("059", $"Erro ao processar dados do campo: {ex.Message}", ExceptionType.Server, "Data"), cancellationToken);
                return null!;
            }

            // Gerar slug para o field
            var fieldSlug = CollectionField.GenerateSlug(field.Name)?.ToLowerInvariant() ?? string.Empty;

            // Criar CollectionField
            var collectionField = new CollectionField
            {
                Id = null,
                Type = field.Type,
                Name = field.Name,
                Slug = fieldSlug,
                Data = bsonDocument,
                CreatedAt = existingCollection.CreatedAt, // Manter a data de criação original
                UpdatedAt = now
            };

            collectionFields.Add(collectionField);

            // Adicionar ao resultado
            fieldResults.Add(new CollectionFieldItemResultDto
            {
                Id = null,
                Type = field.Type,
                Name = field.Name,
                Slug = fieldSlug,
                Data = data
            });
        }

        // Gerar slug único para a collection (se o nome mudou)
        string collectionSlug;
        if (request.Name != existingCollection.Name)
        {
            collectionSlug = await GenerateUniqueCollectionSlugAsync(request.Name, request.UserData.StageId, cancellationToken);
        }
        else
        {
            collectionSlug = existingCollection.Slug; // Manter o slug existente
        }

        // Atualizar a Collection
        existingCollection.Name = request.Name;
        existingCollection.Slug = collectionSlug;
        existingCollection.Fields = collectionFields;
        existingCollection.UpdatedAt = now;

        try
        {
            await _collectionRepository.UpdateAsync(existingCollection, cancellationToken);
            _logger.LogInformation("Collection atualizada com sucesso. Id: {CollectionId}, Name: {Name}", existingCollection.Id, existingCollection.Name);

            return new UpdateCollectionCommandResult
            {
                Id = existingCollection.Id,
                Fields = fieldResults,
                CreatedAt = existingCollection.CreatedAt,
                UpdatedAt = existingCollection.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar collection. Exception: {Exception}", ex.ToString());
            await _bus.Publish(new ExceptionNotification("057", $"Erro ao atualizar collection: {ex.Message}", ExceptionType.Server, "Collection"), cancellationToken);
            return null!;
        }
    }

    private async Task<string> GenerateUniqueCollectionSlugAsync(string name, string stageId, CancellationToken cancellationToken)
    {
        var baseSlug = CollectionAggregate.GenerateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        var existingCollection = await _collectionRepository.GetBySlugAndStageIdAsync(slug, stageId, cancellationToken);

        if (existingCollection != null)
        {
            var existingSlug = existingCollection.Slug;
            var match = Regex.Match(existingSlug, @"^(.+)_(\d+)$");

            if (match.Success)
            {
                var basePart = match.Groups[1].Value;
                var numberPart = int.Parse(match.Groups[2].Value);
                counter = numberPart + 1;
                slug = $"{basePart}_{counter}";
            }
            else
            {
                slug = $"{baseSlug}_1";
                counter = 1;
            }

            while (true)
            {
                existingCollection = await _collectionRepository.GetBySlugAndStageIdAsync(slug, stageId, cancellationToken);
                if (existingCollection == null)
                {
                    break;
                }

                var existingSlugCheck = existingCollection.Slug;
                var matchCheck = Regex.Match(existingSlugCheck, @"^(.+)_(\d+)$");

                if (matchCheck.Success)
                {
                    var numberPartCheck = int.Parse(matchCheck.Groups[2].Value);
                    counter = numberPartCheck + 1;
                }
                else
                {
                    counter++;
                }

                slug = $"{baseSlug}_{counter}";
            }
        }

        slug = slug?.ToLowerInvariant() ?? string.Empty;
        _logger.LogInformation("Slug único da collection gerado: {Slug} (base: {BaseSlug}, stageId: {StageId})", slug, baseSlug, stageId);
        return slug;
    }
}

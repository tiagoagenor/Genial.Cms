using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Commands.TypeFile;
using Genial.Cms.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.CommandHandlers.TypeFile;

public class GetTypeFilesCommandHandler : IRequestHandler<GetTypeFilesCommand, GetTypeFilesResponse>
{
    private readonly ITypeFileRepository _typeFileRepository;
    private readonly ILogger<GetTypeFilesCommandHandler> _logger;

    public GetTypeFilesCommandHandler(
        ITypeFileRepository typeFileRepository,
        ILogger<GetTypeFilesCommandHandler> logger)
    {
        _typeFileRepository = typeFileRepository;
        _logger = logger;
    }

    public async Task<GetTypeFilesResponse> Handle(GetTypeFilesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando todos os TypeFiles");

        var typeFiles = await _typeFileRepository.GetAllAsync(cancellationToken);

        if (typeFiles == null || typeFiles.Count == 0)
        {
            _logger.LogInformation("Nenhum TypeFile encontrado");
            return new GetTypeFilesResponse
            {
                Data = new List<TypeFileDto>()
            };
        }

        var result = new GetTypeFilesResponse
        {
            Data = typeFiles
                .OrderBy(tf => tf.Order)
                .Select(tf => new TypeFileDto
                {
                    Id = tf.Id,
                    Key = tf.Key,
                    Value = tf.Value,
                    Order = tf.Order,
                    Category = tf.Category,
                    CreatedAt = tf.CreatedAt,
                    UpdatedAt = tf.UpdatedAt
                }).ToList()
        };

        _logger.LogInformation("TypeFiles encontrados: {Count}", result.Data.Count);
        return result;
    }
}

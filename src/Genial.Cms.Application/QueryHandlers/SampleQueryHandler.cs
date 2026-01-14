using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Queries;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Genial.Cms.Application.QueryHandlers;

public class SampleQueryHandler : QueryHandler<SampleQuery, IEnumerable<string>>
{
    public SampleQueryHandler(DatabaseConfiguration databaseConfiguration, IMediator bus, ILogger logger) : base(databaseConfiguration, bus, logger)
    {
    }

    public override async Task<IEnumerable<string>> Handle(SampleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.ForceClientException)
            {
                await Bus.Publish(new ExceptionNotification("0001", "Error on processing the request, check your request parameters", ExceptionType.Client), cancellationToken);
                return default;
            }

            return new List<string> {"Nothing", "Here", "Just", "Hello"};
        }
        catch (Exception ex)
        {
            await Bus.Publish(new ExceptionNotification("0002", "Error on processing the request", ExceptionType.Server), cancellationToken);
            Logger.LogCritical("Error on processing the sample request #### Exception: {Exception} ####", ex);
            return default;
        }
    }
}

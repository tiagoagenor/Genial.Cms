using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Application.Queries;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;
using MediatR;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace Genial.Cms.Application.QueryHandlers;

public abstract class QueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : Query<TResponse>
{
    protected readonly DatabaseConfiguration DatabaseConfiguration;
    protected readonly IMediator Bus;
    protected readonly ILogger Logger;

    protected QueryHandler(DatabaseConfiguration databaseConfiguration, IMediator bus, ILogger logger)
    {
        DatabaseConfiguration = databaseConfiguration;
        Bus = bus;
        Logger = logger;
    }

    public abstract Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken);

    protected IDbConnection CreateDatabaseConnection()
    {
        Logger.LogInformation("Inicializando conexão com o banco de dados");
        return new OracleConnection(DatabaseConfiguration.ConnectionString);
    }
}

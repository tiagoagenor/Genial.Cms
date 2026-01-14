using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Genial.Cms.Infra.Data.Context;

public class ApplicationDbContext : DbContext
{
    private readonly IMediator _bus;
    private readonly DatabaseConfiguration _databaseConfiguration;


    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator, DatabaseConfiguration databaseConfiguration) : base(options)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        _bus = mediator;
        _databaseConfiguration = databaseConfiguration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseOracle(_databaseConfiguration.ConnectionString, o => o.UseOracleSQLCompatibility("11"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await _bus.DispatchDomainEventsAsync(this);

        await base.SaveChangesAsync(cancellationToken);

        return true;
    }
}

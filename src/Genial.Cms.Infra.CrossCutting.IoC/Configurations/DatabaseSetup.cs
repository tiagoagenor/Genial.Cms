using System;
using Genial.Cms.Domain.SeedWork;
using Genial.Cms.Infra.Data.Context;
using Genial.Cms.Infra.Data.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace Genial.Cms.Infra.CrossCutting.IoC.Configurations;

public static class DatabaseSetup
{
	public static void AddDatabaseSetup(this IServiceCollection services)
	{
        ArgumentNullException.ThrowIfNull(services);

		services.AddDbContext<ApplicationDbContext>(ServiceLifetime.Scoped);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<MongoDbContext>();
	}
}

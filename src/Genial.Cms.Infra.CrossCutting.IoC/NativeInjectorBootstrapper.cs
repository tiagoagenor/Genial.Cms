using System.Collections.Generic;
using Genial.Cms.Application.Behaviors;
using Genial.Cms.Application.CommandHandlers;
using Genial.Cms.Application.Services;
using Genial.Cms.Domain.Exceptions;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genial.Cms.Infra.CrossCutting.IoC;

public static class NativeInjectorBootstrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        RegisterMediator(services);
        RegisterEnvironments(services, configuration);
        RegisterApplicationServices(services);
    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IFilesStorageConfigurationService, FilesStorageConfigurationService>();
        services.AddMemoryCache(); // Necessário para o cache do FilesStorageConfigurationService
    }

    private static void RegisterMediator(IServiceCollection services)
    {
        services.AddMediatR(c =>
        {
            c.RegisterServicesFromAssemblyContaining(typeof(CommandHandler<,>));
            c.AddOpenBehavior(typeof(ValidatorBehavior<,>));
            c.Lifetime = ServiceLifetime.Scoped;
        });

        services.AddScoped<INotificationHandler<ExceptionNotification>, ExceptionNotificationHandler>();
    }

    private static void RegisterEnvironments(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration.GetSection(nameof(DatabaseConfiguration)).Get<DatabaseConfiguration>());

        var mongoDbConfiguration = configuration.GetSection("MongoDb").Get<MongoDbConfiguration>()
            ?? new MongoDbConfiguration();
        services.AddSingleton(mongoDbConfiguration);

        var jwtConfiguration = configuration.GetSection("Jwt").Get<JwtConfiguration>()
            ?? new JwtConfiguration();
        services.AddSingleton(jwtConfiguration);

        var seedUsers = configuration.GetSection("SeedUsers").Get<List<SeedUserConfiguration>>()
            ?? new List<SeedUserConfiguration>();
        var seedConfiguration = new SeedConfiguration { SeedUsers = seedUsers };
        services.AddSingleton(seedConfiguration);
    }
}

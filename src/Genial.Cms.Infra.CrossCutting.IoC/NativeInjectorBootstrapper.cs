using Genial.Cms.Application.Behaviors;
using Genial.Cms.Application.CommandHandlers;
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
    }
}

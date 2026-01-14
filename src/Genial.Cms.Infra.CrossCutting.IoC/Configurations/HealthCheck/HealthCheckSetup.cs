using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;

namespace Genial.Cms.Infra.CrossCutting.IoC.Configurations.HealthCheck;

public static class HealthCheckSetup
{
    public static void AddHealthCheck(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var hcBuilder = services.AddHealthChecks();

        hcBuilder.AddCheck("SELF CHECK API", () => HealthCheckResult.Healthy("HealthCheck Working For Genial.Cms"));

        hcBuilder.AddCheck<RequiredSectionsHealthCheck<DatabaseConfiguration>>(nameof(DatabaseConfiguration));

        var applicationConfiguration = serviceProvider.GetRequiredService<DatabaseConfiguration>();
        hcBuilder.AddOracle(applicationConfiguration.ConnectionString, name: "ORACLE HEALTHCHECK", timeout: TimeSpan.FromSeconds(10));
    }

    public static void MapHealthCheck(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/_health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/_live", new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("SELF")
        });
    }
}

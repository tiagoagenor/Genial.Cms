using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Genial.Cms.Infra.CrossCutting.IoC.Configurations.HealthCheck;

internal class RequiredVariablesHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var variable = Environment.GetEnvironmentVariable(context.Registration.Name);

        return Task.FromResult(
            !string.IsNullOrWhiteSpace(variable)
                ? HealthCheckResult.Healthy("Variable mapped")
                : HealthCheckResult.Unhealthy($"Variable not mapped: {context.Registration.Name}")
        );
    }
}

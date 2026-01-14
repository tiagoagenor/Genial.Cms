using System;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Genial.Cms.Infra.CrossCutting.IoC.Configurations;

public static class HttpClientSetup
{
    public static void AddHttpClientProxy<TInterface, TConfiguration>(this IServiceCollection services, IConfiguration configuration)
        where TConfiguration : BaseHttpClientProxyConfiguration
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(typeof(TConfiguration).Name);

        if (!section.Exists()) throw new ArgumentNullException($"Section {typeof(TConfiguration).Name} not properly configured");

        var proxyConfiguration = section.Get<TConfiguration>();

        services.AddRefitClient<TInterface>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(proxyConfiguration.BaseUrl));
    }
}

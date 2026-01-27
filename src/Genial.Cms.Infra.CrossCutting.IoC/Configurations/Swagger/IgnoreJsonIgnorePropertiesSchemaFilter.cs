using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Genial.Cms.Infra.CrossCutting.IoC.Configurations.Swagger;

internal class IgnoreJsonIgnorePropertiesSchemaFilter : ISchemaFilter
{
    private readonly JsonNamingPolicy _namingPolicy = JsonNamingPolicy.CamelCase;

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || context.Type == null)
        {
            return;
        }

        var propertiesToRemove = context.Type
            .GetProperties()
            .Where(prop => prop.GetCustomAttribute<JsonIgnoreAttribute>() != null)
            .Select(prop => 
            {
                // Obter o nome da propriedade como aparece no JSON
                var jsonPropertyNameAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jsonPropertyNameAttr != null)
                {
                    return jsonPropertyNameAttr.Name;
                }
                
                // Se não tiver JsonPropertyName, usar camelCase (padrão do ASP.NET Core)
                return _namingPolicy.ConvertName(prop.Name);
            })
            .ToList();

        foreach (var propertyName in propertiesToRemove)
        {
            // Tentar remover com o nome exato e também com diferentes variações de case
            var keysToRemove = schema.Properties.Keys
                .Where(key => string.Equals(key, propertyName, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                schema.Properties.Remove(key);
                // Também remover de Required se estiver lá
                if (schema.Required != null)
                {
                    schema.Required.Remove(key);
                }
            }
        }
    }
}

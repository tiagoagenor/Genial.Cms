using System;
using System.Text;
using System.Threading.Tasks;
using Genial.Cms.Infra.CrossCutting.Environments.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Genial.Cms.Infra.CrossCutting.IoC.Configurations;

public static class JwtAuthenticationSetup
{
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfiguration = configuration.GetSection("Jwt").Get<JwtConfiguration>()
            ?? new JwtConfiguration();

        var key = Encoding.UTF8.GetBytes(jwtConfiguration.SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtConfiguration.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtConfiguration.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("JwtAuthentication");
                    logger.LogError(context.Exception, "Falha na autenticação JWT: {Error}", context.Exception?.Message);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("JwtAuthentication");
                    logger.LogWarning("Desafio de autenticação JWT. Error: {Error}, ErrorDescription: {ErrorDescription}", 
                        context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("JwtAuthentication");
                    logger.LogInformation("Token JWT validado com sucesso para usuário: {UserId}", 
                        context.Principal?.FindFirst("userId")?.Value);
                    return Task.CompletedTask;
                }
            };
        });
    }
}


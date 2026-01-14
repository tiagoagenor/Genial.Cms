using System;
using Genial.Cms.Application.Validators;
using Genial.Cms.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations.HealthCheck;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations.Swagger;
using FluentValidation;
using Genial.Arquitetura.LoggerActionAPI.Extensions;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging para exibir no console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddValidatorsFromAssemblyContaining(typeof(Validator<>), ServiceLifetime.Singleton);
builder.Services.AddLoggerAction(builder.Configuration);
builder.Services.AddApiVersioning();
builder.Services.AddVersionedApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerSetup();
builder.Services.AddScoped<GlobalExceptionFilterAttribute>();
builder.Services.AddDependencyInjectionSetup(builder.Configuration);
builder.Services.AddDatabaseSetup();
builder.Services.AddHealthCheck();

var app = builder.Build();

app.UseCors(corsBuilder =>
{
    corsBuilder.WithOrigins("*");
    corsBuilder.AllowAnyOrigin();
    corsBuilder.AllowAnyMethod();
    corsBuilder.AllowAnyHeader();
});

// Middleware para capturar exceções não tratadas e logar no console
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception: {Message}\nStack Trace: {StackTrace}",
            ex.Message, ex.StackTrace);

        if (ex.InnerException != null)
        {
            logger.LogError("Inner Exception: {InnerMessage}\nInner Stack Trace: {InnerStackTrace}",
                ex.InnerException.Message, ex.InnerException.StackTrace);
        }

        throw; // Re-throw para que o GlobalExceptionFilterAttribute possa tratar
    }
});

app.UseRouting();
app.UseLoggerAction(builder.Configuration);

app.MapSwagger();
app.MapControllers();
app.MapHealthCheck();

app.Run();

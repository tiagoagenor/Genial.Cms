using System;
using System.IO;
using Genial.Cms.Application.Validators;
using Genial.Cms.Filters;
using Genial.Cms.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations.HealthCheck;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations.Swagger;
using FluentValidation;
using Genial.Arquitetura.LoggerActionAPI.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel para aceitar requisições grandes (até 2GB)
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024; // 2GB
});

// Configurar logging para exibir no console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddValidatorsFromAssemblyContaining(typeof(Validator<>), ServiceLifetime.Singleton);
builder.Services.AddLoggerAction(builder.Configuration);
builder.Services.AddApiVersioning();
builder.Services.AddVersionedApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtUserService, JwtUserService>();
builder.Services.AddControllers();
// Configurar tamanho máximo de requisição para uploads (2GB)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024; // 2GB
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
});

// Configurar CORS para permitir qualquer origem
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerSetup();
builder.Services.AddScoped<GlobalExceptionFilterAttribute>();
builder.Services.AddDependencyInjectionSetup(builder.Configuration);
builder.Services.AddDatabaseSetup();
builder.Services.AddHealthCheck();
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Aplicar política CORS (permite qualquer origem)
app.UseCors();

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
app.UseAuthentication();
app.UseAuthorization();
app.UseLoggerAction(builder.Configuration);

// Configurar servidor de arquivos estáticos para uploads
var uploadsPath = builder.Configuration["FileUpload:Path"] ?? "uploads";
var uploadsFullPath = Path.Combine(builder.Environment.ContentRootPath, uploadsPath);
if (!Directory.Exists(uploadsFullPath))
{
    Directory.CreateDirectory(uploadsFullPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsFullPath),
    RequestPath = $"/{uploadsPath}"
});

app.MapSwagger();
app.MapControllers();
app.MapHealthCheck();

app.Run();

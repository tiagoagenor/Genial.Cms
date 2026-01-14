using Genial.Cms.Application.Validators;
using Genial.Cms.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations.HealthCheck;
using Genial.Cms.Infra.CrossCutting.IoC.Configurations.Swagger;
using FluentValidation;
// using Genial.Arquitetura.LoggerActionAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining(typeof(Validator<>), ServiceLifetime.Singleton);
// builder.Services.AddLoggerAction(builder.Configuration);
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
app.UseRouting();
// app.UseLoggerAction(builder.Configuration);

app.MapSwagger();
app.MapControllers();
app.MapHealthCheck();

app.Run();

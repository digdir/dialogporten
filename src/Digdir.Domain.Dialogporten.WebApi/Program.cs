using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.WebApi;
using FastEndpoints;
using FastEndpoints.Swagger;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddFastEndpoints()
    .AddSwaggerDoc(
        maxEndpointVersion: 1,
        shortSchemaNames: true,
        settings: s =>
        {
            s.Title = "Dialogporten";
            s.DocumentName = "V1.0";
            s.Version = "v1.0";
        })
    .AddApplication(x => builder.Configuration.Bind(nameof(ApplicationSettings), x))
    .AddInfrastructure(x => builder.Configuration.Bind(nameof(InfrastructureSettings), x));

var app = builder.Build();

app.UseProblemDetailsExceptionHandler()
    .UseHttpsRedirection()
    .UseAuthorization()
    .UseFastEndpoints(x =>
    {
        x.Endpoints.RoutePrefix = "api";
        x.Versioning.Prefix = "v";
        x.Versioning.PrependToRoute = true;
        x.Versioning.DefaultVersion = 1;
        x.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
        x.Errors.ResponseBuilder = ErrorResponseBuilderExtensions.ResponseBuilder;
    })
    .UseOpenApi()
    .UseSwaggerUi3(x => x.ConfigureDefaults());

app.Run();

using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc(maxEndpointVersion: 1, settings: s =>
{
    s.DocumentName = "Initial Release";
    s.Title = "my api";
    s.Version = "v1.0";
});
builder.Services.AddApplication(x => builder.Configuration.Bind(nameof(ApplicationSettings), x));
builder.Services.AddInfrastructure(x => builder.Configuration.Bind(nameof(InfrastructureSettings), x));

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseFastEndpoints(x =>
{
    x.Endpoints.RoutePrefix = "api";
    x.Versioning.Prefix = "v";
    x.Versioning.PrependToRoute = true;
    x.Versioning.DefaultVersion = 1;
});
app.UseOpenApi();
app.UseSwaggerUi3(x => x.ConfigureDefaults());
app.Run();


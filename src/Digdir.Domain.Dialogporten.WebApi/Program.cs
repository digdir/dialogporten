using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;
using Digdir.Domain.Dialogporten.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication(x => builder.Configuration.Bind(nameof(ApplicationSettings), x));
builder.Services.AddInfrastructure(x => builder.Configuration.Bind(nameof(InfrastructureSettings), x));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", async (ISender sender) =>
{
    var dialogue = await sender.Send(new GetDialogueQuery { Id = Guid.NewGuid() });
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
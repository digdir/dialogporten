using Altinn.ApiClients.Dialogporten.Config;
using Altinn.ApiClients.Dialogporten.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.local.json", optional: false, reloadOnChange: true)
    .Build();
var settings = configuration.GetSection("DialogportenSettings").Get<DialogportenSettings>()!;

services.AddControllers();
services.AddRouting();
services.AddSingleton<IConfiguration>(configuration);
services.AddDialogportenClient(settings);

var app = builder.Build();
app.MapControllers();
app.UseRouting();
app.Run();

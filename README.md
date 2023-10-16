# Dialogporten

## Getting started with local development

### Prerequisites
- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Docker](https://www.docker.com/products/docker-desktop/) (Docker Desktop version 4.22 or later or Docker Compose version 2.20 or later)


You can run the entire project locally using docker compose.
```powershell
docker compose up
```

If you need do debug the WebApi project in an IDE, you can alternatively run docker compose without the WebAPI.  
First create a dotnet user secret for the DB connection string.

```powerhell
dotnet user-secrets set -p .\src\Digdir.Domain.Dialogporten.WebApi\ "Infrastructure:DialogDbConnectionString" "Server=localhost;Port=5432;Database=Dialogporten;User ID=postgres;Password=supersecret;"
```

Then run docker compose without the WebAPI project.
```powershell
docker compose -f docker-compose-no-webapi.yaml up 
```


## DB development
This project uses Entity Framework core to manage DB migrations. DB development can ether be done through Visual Studios Package Manager Console (PMC), or through the CLI. 

### DB development through PMC
Set Digdir.Domain.Dialogporten.Infrastructure as startup project in Visual Studios solution explorer, and as default project in PMC. You are now ready to use [EF core tools through PMC](https://learn.microsoft.com/en-us/ef/core/cli/powershell). Run the following command for more information:
```powershell
Get-Help about_EntityFrameworkCore
```

### DB development through CLI
Install the CLI tool with the following command:
```powershell
dotnet tool install --global dotnet-ef
```

You are now ready to use [EF core tools through CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet). Run the following command for more information:
```powershell
dotnet ef --help
```

Remember to target `Digdir.Domain.Dialogporten.Infrastructure` project when running the CLI commands. Either target it through the command using the `-p` option, i.e.
```powershell
dotnet ef migrations add -p .\src\Digdir.Domain.Dialogporten.Infrastructure\ TestMigration
```

or change your directory to the infrastructure project, and then run the command.
```powershell
cd .\src\Digdir.Domain.Dialogporten.Infrastructure\
dotnet ef migrations add TestMigration
```

## Development in local and test environments
To generate test tokens see https://github.com/Altinn/AltinnTestTools. There is a request in the Postman collection for this.

### Local development settings
We are able to toggle some external resources in local development. This is done through the `appsettings.Development.json` file. The following settings are available:
```json
"LocalDevelopment": {
	"UseLocalDevelopmentUser": true,
	"UseLocalDevelopmentResourceRegister": true,
	"UseLocalDevelopmentCloudEventBus": true,
	"DisableAuth": true
}
```
Toggling these flags will enable/disable the external resources. The `DisableAuth` flag, for example, will disable authentication in the WebAPI project. This is useful when debugging the WebAPI project in an IDE. These settings will only be respected in the `Development` environment.
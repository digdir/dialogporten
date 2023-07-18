# Dialogporten

## Getting started with local development

### Prerequisites
- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Docker](https://www.docker.com/products/docker-desktop/)

1. Create a postgres instance in your local docker
2. Add the connection string to your local secrets
3. Run the web api through your favorite IDE, or through the CLI

```powershell
# 1. Create a postgres instance in your local docker
docker run --name DialogportenLocalPostgreSql -p 5432:5432 -e POSTGRES_PASSWORD=supersecret -e POSTGRES_USER=postgres -d postgres:14.7 -c wal_level=logical

# 2. Add the connection string to your local secrets
dotnet user-secrets set -p .\src\Digdir.Domain.Dialogporten.WebApi\ "Infrastructure:DialogDbConnectionString" "Server=localhost;Port=5432;Database=Dialogporten;User ID=postgres;Password=supersecret;"

# 3. Run the web api through your favorite IDE, or through the CLI
dotnet run --project .\src\Digdir.Domain.Dialogporten.WebApi\
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

Remember to target `Digdir.Domain.Dialogporten.Infrastructure` project when running the CLI commands. Ether target it throguh the command using the `-p` option, i.e.
```powershell
dotnet ef migrations add -p .\src\Digdir.Domain.Dialogporten.Infrastructure\ TestMigration
```

or change your directory to the infrastructure project, and then run the command.
```powershell
cd .\src\Digdir.Domain.Dialogporten.Infrastructure\
dotnet ef migrations add TestMigration
```

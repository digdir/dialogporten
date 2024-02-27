# Dialogporten

## Getting started with local development

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Podman](https://podman.io/)

#### Installing Podman (Mac)

1. a) If installed, uninstall Docker
1. b) Install [Podman](https://podman.io/)

2. Install dependencies:
```bash
brew tap cfergeau/crc
brew install vfkit
brew install podman-compose
```

3. Restart your Mac

4. Finish setup in Podman Desktop

5. Enable `Docker Compatility mode` by clicking on the `Docker compatibility` on the bottom left corner

6. Enable privileged [testcontainers-dotnet](https://github.com/testcontainers/testcontainers-dotnet/issues/876#issuecomment-1930397928)  
`echo "ryuk.container.privileged = true" >> $HOME/.testcontainers.properties`

#### Installing Podman (Windows)

TODO

#### Verify prerequisites

You can run the entire project locally using `podman compose`.
```powershell
podman compose up
```
The APIs SwaggerUI should now be available at [localhost:7124/swagger](https://localhost:7214/swagger/index.html)


### Running the WebApi in an IDE
If you need do debug the WebApi project in an IDE, you can alternatively run `podman compose` without the WebAPI.  
First create a dotnet user secret for the DB connection string.

```powershell
dotnet user-secrets set -p .\src\Digdir.Domain.Dialogporten.WebApi\ "Infrastructure:DialogDbConnectionString" "Server=localhost;Port=5432;Database=Dialogporten;User ID=postgres;Password=supersecret;"
```

Then run `podman compose` without the WebAPI project.
```powershell
podman compose -f docker-compose-no-webapi.yml up 
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
## Testing

Besides ordinary unit and integration tests, there are test suites for both functional and non-functional end-to-end tests implemented with [K6](https://k6.io/).

See `tests/k6/README.md` for more information.

## Development in local and test environments
To generate test tokens see, https://github.com/Altinn/AltinnTestTools. There is a request in the Postman collection for this.

### Local development settings
We are able to toggle some external resources in local development. This is done through the `appsettings.Development.json` file. The following settings are available:
```json
"LocalDevelopment": {
    "UseLocalDevelopmentUser": true,
    "UseLocalDevelopmentResourceRegister": true,
    "UseLocalDevelopmentOrganizationRegister": true,
    "UseLocalDevelopmentNameRegister": true,
    "UseLocalDevelopmentAltinnAuthorization": true,
    "UseLocalDevelopmentCloudEventBus": true,
    "DisableShortCircuitOutboxDispatcher": true,
    "DisableAuth": true
}
```
Toggling these flags will enable/disable the external resources. The `DisableAuth` flag, for example, will disable authentication in the WebAPI project. This is useful when debugging the WebAPI project in an IDE. These settings will only be respected in the `Development` environment.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/**/*.csproj", "./"]
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore "src/Digdir.Tool.Dialogporten.MigrationVerifier/Digdir.Tool.Dialogporten.MigrationVerifier.csproj"

COPY . .

WORKDIR "/src/src/Digdir.Tool.Dialogporten.MigrationVerifier"
RUN dotnet publish "Digdir.Tool.Dialogporten.MigrationVerifier.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final

RUN useradd appuser && chown -R appuser /app

USER appuser

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Digdir.Tool.Dialogporten.MigrationVerifier.dll"]
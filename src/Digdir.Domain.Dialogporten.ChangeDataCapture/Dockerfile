FROM mcr.microsoft.com/dotnet/aspnet:8.0.10@sha256:3ded9ccf06f222ec934311be4f9facda83d144331c028340e3a694733cad7d4b AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.403@sha256:cab0284cce7bc26d41055d0ac5859a69a8b75d9a201cd226999f4f00cc983f13 AS build
WORKDIR /src

COPY [".editorconfig", "."]
COPY ["Directory.Build.props", "."]

# Main project
COPY ["src/Digdir.Domain.Dialogporten.ChangeDataCapture/Digdir.Domain.Dialogporten.ChangeDataCapture.csproj", "src/Digdir.Domain.Dialogporten.ChangeDataCapture/"]
# Dependencies
COPY ["src/Digdir.Domain.Dialogporten.Domain/Digdir.Domain.Dialogporten.Domain.csproj", "src/Digdir.Domain.Dialogporten.Domain/"]
COPY ["src/Digdir.Library.Entity.Abstractions/Digdir.Library.Entity.Abstractions.csproj", "src/Digdir.Library.Entity.Abstractions/"]
# Restore
RUN dotnet restore "src/Digdir.Domain.Dialogporten.ChangeDataCapture/Digdir.Domain.Dialogporten.ChangeDataCapture.csproj"
# Copy source
COPY ["src/", "."]
# Publish
WORKDIR "/src/Digdir.Domain.Dialogporten.ChangeDataCapture"
RUN dotnet publish "Digdir.Domain.Dialogporten.ChangeDataCapture.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Digdir.Domain.Dialogporten.ChangeDataCapture.dll"]

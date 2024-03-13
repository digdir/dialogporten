FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:3ff67792728179308c4bf06799d8b45d155c60fddc347acf69465a496d9a20b8 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:aabbef174ecb15ef9047574ae2d45a82479e670f62c0a1ad1fa6d623ee19e623 AS build
WORKDIR /src
# Main project
COPY ["src/Digdir.Domain.Dialogporten.WebApi/Digdir.Domain.Dialogporten.WebApi.csproj", "src/Digdir.Domain.Dialogporten.WebApi/"]
# Dependencies
COPY ["src/Digdir.Domain.Dialogporten.Application/Digdir.Domain.Dialogporten.Application.csproj", "src/Digdir.Domain.Dialogporten.Application/"]
COPY ["src/Digdir.Domain.Dialogporten.Domain/Digdir.Domain.Dialogporten.Domain.csproj", "src/Digdir.Domain.Dialogporten.Domain/"]
COPY ["src/Digdir.Library.Entity.Abstractions/Digdir.Library.Entity.Abstractions.csproj", "src/Digdir.Library.Entity.Abstractions/"]
COPY ["src/Digdir.Library.Entity.EntityFrameworkCore/Digdir.Library.Entity.EntityFrameworkCore.csproj", "src/Digdir.Library.Entity.EntityFrameworkCore/"]
COPY ["src/Digdir.Domain.Dialogporten.Infrastructure/Digdir.Domain.Dialogporten.Infrastructure.csproj", "src/Digdir.Domain.Dialogporten.Infrastructure/"]
# Restore project
RUN dotnet restore "src/Digdir.Domain.Dialogporten.WebApi/Digdir.Domain.Dialogporten.WebApi.csproj"
# Copy source
COPY ["src/", "."]
# Publish
WORKDIR "/src/Digdir.Domain.Dialogporten.WebApi"
RUN dotnet publish "Digdir.Domain.Dialogporten.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Digdir.Domain.Dialogporten.WebApi.dll"]

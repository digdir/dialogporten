FROM mcr.microsoft.com/dotnet/aspnet:8.0.8@sha256:38178966094e8ac38ac3377fa4195cbf63e9aef4030f5e62d2810f8a5df769e4 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.401@sha256:8c6beed050a602970c3d275756ed3c19065e42ce6ca0809f5a6fcbf5d36fd305 AS build
WORKDIR /src

COPY [".editorconfig", "."]
COPY ["Directory.Build.props", "."]

# Main project
COPY ["src/Digdir.Domain.Dialogporten.Service/Digdir.Domain.Dialogporten.Service.csproj", "src/Digdir.Domain.Dialogporten.Service/"]
# Dependencies
COPY ["src/Digdir.Domain.Dialogporten.Application/Digdir.Domain.Dialogporten.Application.csproj", "src/Digdir.Domain.Dialogporten.Application/"]
COPY ["src/Digdir.Domain.Dialogporten.Domain/Digdir.Domain.Dialogporten.Domain.csproj", "src/Digdir.Domain.Dialogporten.Domain/"]
COPY ["src/Digdir.Library.Entity.Abstractions/Digdir.Library.Entity.Abstractions.csproj", "src/Digdir.Library.Entity.Abstractions/"]
COPY ["src/Digdir.Library.Entity.EntityFrameworkCore/Digdir.Library.Entity.EntityFrameworkCore.csproj", "src/Digdir.Library.Entity.EntityFrameworkCore/"]
COPY ["src/Digdir.Domain.Dialogporten.Infrastructure/Digdir.Domain.Dialogporten.Infrastructure.csproj", "src/Digdir.Domain.Dialogporten.Infrastructure/"]
# Restore
RUN dotnet restore "src/Digdir.Domain.Dialogporten.Service/Digdir.Domain.Dialogporten.Service.csproj"
# Copy rest of source
COPY ["src/", "/src/"]
# Publish
WORKDIR "/src/Digdir.Domain.Dialogporten.Service"
RUN dotnet publish "Digdir.Domain.Dialogporten.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Digdir.Domain.Dialogporten.Service.dll"]

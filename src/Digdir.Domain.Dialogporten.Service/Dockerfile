FROM mcr.microsoft.com/dotnet/aspnet:8.0.7@sha256:3deda593cf10581cbacfa16a1fbb090353d14beaa65adca4611c7c7a458d66b0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.303@sha256:7d0ba26469267b563120456557e38eccef9972cb6b9cfbbd47a50d1218fa7b30 AS build
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

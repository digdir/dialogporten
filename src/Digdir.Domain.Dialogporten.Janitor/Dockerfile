FROM mcr.microsoft.com/dotnet/aspnet:9.0.0@sha256:d8f01f752bf9bd3ff630319181a2ccfbeecea4080a1912095a34002f61bfa345 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0.101@sha256:fe8ceeca5ee197deba95419e3b85c32744970b730ae11645e13f1cb74a848d98 AS build
WORKDIR /src

COPY [".editorconfig", "."]
COPY ["Directory.Build.props", "."]

# Main project and dependencies
COPY ["src/Digdir.Domain.Dialogporten.Janitor/Digdir.Domain.Dialogporten.Janitor.csproj", "src/Digdir.Domain.Dialogporten.Janitor/"]
COPY ["src/Digdir.Domain.Dialogporten.Application/Digdir.Domain.Dialogporten.Application.csproj", "src/Digdir.Domain.Dialogporten.Application/"]
COPY ["src/Digdir.Domain.Dialogporten.Domain/Digdir.Domain.Dialogporten.Domain.csproj", "src/Digdir.Domain.Dialogporten.Domain/"]
COPY ["src/Digdir.Library.Entity.Abstractions/Digdir.Library.Entity.Abstractions.csproj", "src/Digdir.Library.Entity.Abstractions/"]
COPY ["src/Digdir.Library.Entity.EntityFrameworkCore/Digdir.Library.Entity.EntityFrameworkCore.csproj", "src/Digdir.Library.Entity.EntityFrameworkCore/"]
COPY ["src/Digdir.Domain.Dialogporten.Infrastructure/Digdir.Domain.Dialogporten.Infrastructure.csproj", "src/Digdir.Domain.Dialogporten.Infrastructure/"]

# Restore project
RUN dotnet restore "src/Digdir.Domain.Dialogporten.Janitor/Digdir.Domain.Dialogporten.Janitor.csproj"

# Copy source
COPY ["src/", "."]

# Publish
WORKDIR "/src/Digdir.Domain.Dialogporten.Janitor"
RUN dotnet publish "Digdir.Domain.Dialogporten.Janitor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Digdir.Domain.Dialogporten.Janitor.dll"]
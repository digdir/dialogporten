FROM mcr.microsoft.com/dotnet/aspnet:8.0.3@sha256:9470bf16cb8566951dfdb89d49a4de73ceb31570b3cdb59059af44fe53b19547 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.203@sha256:249a78aa4ce22ab872d0dff0490a6389e7bc087d2c080c4ffc7569b49cf0e23b AS build
WORKDIR /src

# Main project
COPY ["src/Digdir.Domain.Dialogporten.Infrastructure/Digdir.Domain.Dialogporten.Infrastructure.csproj", "src/Digdir.Domain.Dialogporten.Infrastructure/"]
# Dependencies 
COPY ["src/Digdir.Domain.Dialogporten.Domain/Digdir.Domain.Dialogporten.Domain.csproj", "src/Digdir.Domain.Dialogporten.Domain/"]
COPY ["src/Digdir.Library.Entity.Abstractions/Digdir.Library.Entity.Abstractions.csproj", "src/Digdir.Library.Entity.Abstractions/"]
COPY ["src/Digdir.Library.Entity.EntityFrameworkCore/Digdir.Library.Entity.EntityFrameworkCore.csproj", "src/Digdir.Library.Entity.EntityFrameworkCore/"]
# Restore
RUN dotnet restore "src/Digdir.Domain.Dialogporten.Infrastructure/Digdir.Domain.Dialogporten.Infrastructure.csproj"
# Copy source
COPY ["src/", "."]

WORKDIR "/src/Digdir.Domain.Dialogporten.Infrastructure"
RUN mkdir -p /app/publish
RUN dotnet tool install --global dotnet-ef --version 7.0.14
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet ef migrations -v bundle -o /app/publish/efbundle

FROM base AS final
ENV Infrastructure__DialogDbConnectionString=""
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ./efbundle -v --connection "${Infrastructure__DialogDbConnectionString}"

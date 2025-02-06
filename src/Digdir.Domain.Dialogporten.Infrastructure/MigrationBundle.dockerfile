FROM mcr.microsoft.com/dotnet/aspnet:9.0.1@sha256:f77d967b8c3ec67b9af4bf577177c1a350e4f769618ff419f632b674edfa9be8 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0.102@sha256:6894a71619e08b47ef9df7ff1f436b21d21db160e5d864e180c294a53d7a12f2 AS build
WORKDIR /src

RUN dotnet tool install --global dotnet-ef
ENV PATH $PATH:/root/.dotnet/tools

COPY [".editorconfig", "."]
COPY ["Directory.Build.props", "."]

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
RUN dotnet ef migrations -v bundle -o /app/publish/efbundle

FROM base AS final
ENV Infrastructure__DialogDbConnectionString=""
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ./efbundle -v --connection "${Infrastructure__DialogDbConnectionString}Command Timeout=0;"

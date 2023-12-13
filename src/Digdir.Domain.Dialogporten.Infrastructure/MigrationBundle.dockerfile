FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/**/*.csproj", "./"]
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore "src/Digdir.Domain.Dialogporten.Infrastructure/Digdir.Domain.Dialogporten.Infrastructure.csproj"

COPY . .

WORKDIR "/src/src/Digdir.Domain.Dialogporten.Infrastructure"
RUN mkdir -p /app/publish
RUN dotnet tool install --global dotnet-ef --version 7.0.14
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet ef migrations -v bundle -o /app/publish/efbundle

FROM base AS final

RUN useradd appuser && chown -R appuser /app

USER appuser
ENV Infrastructure__DialogDbConnectionString=""

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ./efbundle -v --connection "${Infrastructure__DialogDbConnectionString}"
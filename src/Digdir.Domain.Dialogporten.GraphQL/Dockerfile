FROM mcr.microsoft.com/dotnet/aspnet:8.0.6@sha256:a22d22bcedc67df31bca96e2cde2dbac2e59913f9ec684612d42dff45722bcc5 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0.302@sha256:02fdc848bbda5d57d9211a72c99bd665b421206002d66b8bc2cc0b2297c227fa AS build
WORKDIR /src

COPY [".editorconfig", "."]
COPY ["Directory.Build.props", "."]

# Main project
COPY ["src/Digdir.Domain.Dialogporten.GraphQL/Digdir.Domain.Dialogporten.GraphQL.csproj", "src/Digdir.Domain.Dialogporten.GraphQL/"]
# Dependencies
COPY ["src/Digdir.Domain.Dialogporten.Application/Digdir.Domain.Dialogporten.Application.csproj", "src/Digdir.Domain.Dialogporten.Application/"]
COPY ["src/Digdir.Domain.Dialogporten.Domain/Digdir.Domain.Dialogporten.Domain.csproj", "src/Digdir.Domain.Dialogporten.Domain/"]
COPY ["src/Digdir.Library.Entity.Abstractions/Digdir.Library.Entity.Abstractions.csproj", "src/Digdir.Library.Entity.Abstractions/"]
COPY ["src/Digdir.Library.Entity.EntityFrameworkCore/Digdir.Library.Entity.EntityFrameworkCore.csproj", "src/Digdir.Library.Entity.EntityFrameworkCore/"]
COPY ["src/Digdir.Domain.Dialogporten.Infrastructure/Digdir.Domain.Dialogporten.Infrastructure.csproj", "src/Digdir.Domain.Dialogporten.Infrastructure/"]
# Restore project
RUN dotnet restore "src/Digdir.Domain.Dialogporten.GraphQL/Digdir.Domain.Dialogporten.GraphQL.csproj"
# Copy source
COPY ["src/", "."]
# Publish
WORKDIR "/src/Digdir.Domain.Dialogporten.GraphQL"
RUN dotnet publish "Digdir.Domain.Dialogporten.GraphQL.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Digdir.Domain.Dialogporten.GraphQL.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:9.0.0@sha256:372b16214ae67e3626a5b1513ade4a530eae10c172d56ce696163b046565fa46 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0.100@sha256:7d24e90a392e88eb56093e4eb325ff883ad609382a55d42f17fd557b997022ca AS build
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

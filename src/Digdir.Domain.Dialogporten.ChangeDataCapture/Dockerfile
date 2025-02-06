FROM mcr.microsoft.com/dotnet/aspnet:9.0.1@sha256:f77d967b8c3ec67b9af4bf577177c1a350e4f769618ff419f632b674edfa9be8 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0.102@sha256:6894a71619e08b47ef9df7ff1f436b21d21db160e5d864e180c294a53d7a12f2 AS build
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

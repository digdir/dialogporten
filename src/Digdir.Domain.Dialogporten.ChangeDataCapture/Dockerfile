FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["src/**/*.csproj", "./"]
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore "src/Digdir.Domain.Dialogporten.ChangeDataCapture/Digdir.Domain.Dialogporten.ChangeDataCapture.csproj"

COPY . .

WORKDIR "/src/src/Digdir.Domain.Dialogporten.ChangeDataCapture"
RUN dotnet build "Digdir.Domain.Dialogporten.ChangeDataCapture.csproj" -c Release -o /app/build
RUN dotnet publish "Digdir.Domain.Dialogporten.ChangeDataCapture.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Digdir.Domain.Dialogporten.ChangeDataCapture.dll"]

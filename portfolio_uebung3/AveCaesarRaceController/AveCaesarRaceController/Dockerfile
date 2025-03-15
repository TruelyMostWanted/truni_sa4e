# Basis-Laufzeitumgebung
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Build-Umgebung
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Nur csproj und Restore für besseres Caching
COPY ["AveCaesarRaceController/AveCaesarRaceController.csproj", "AveCaesarRaceController/"]
RUN dotnet restore "AveCaesarRaceController/AveCaesarRaceController.csproj"

# Restlichen Code kopieren und bauen
COPY . .
WORKDIR "/src/AveCaesarRaceController"
RUN dotnet build "AveCaesarRaceController.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Veröffentlichung des Builds
FROM build AS publish
RUN dotnet publish "AveCaesarRaceController.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Finale Laufzeitumgebung
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Port öffnen (falls erforderlich)
EXPOSE 5000

ENTRYPOINT ["dotnet", "AveCaesarRaceController.dll"]
# Esta etapa se utiliza para construir el proyecto del servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TurneroApp.API/TurneroApp.API.csproj", "TurneroApp.API/"]
RUN dotnet restore "./TurneroApp.API/TurneroApp.API.csproj"
COPY . .
WORKDIR "/src/TurneroApp.API"
RUN dotnet build "./TurneroApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta etapa se utiliza para publicar el proyecto del servicio que se copiará a la etapa final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TurneroApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta etapa se utiliza en producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TurneroApp.API.dll"]
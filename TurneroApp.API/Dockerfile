

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TurneroApp.API/TurneroApp.API.csproj", "TurneroApp.API/"]
RUN dotnet restore "./TurneroApp.API/TurneroApp.API.csproj"
COPY . .
WORKDIR "/src/TurneroApp.API"
RUN dotnet build "./TurneroApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TurneroApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TurneroApp.API.dll"]
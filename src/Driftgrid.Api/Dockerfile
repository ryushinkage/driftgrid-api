FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Driftgrid.Api/Driftgrid.Api.csproj", "src/Driftgrid.Api/"]
COPY ["src/Driftgrid.Application/Driftgrid.Application.csproj", "src/Driftgrid.Application/"]
COPY ["src/Driftgrid.Domain/Driftgrid.Domain.csproj", "src/Driftgrid.Domain/"]
COPY ["src/Driftgrid.Infrastructure/Driftgrid.Infrastructure.csproj", "src/Driftgrid.Infrastructure/"]
RUN dotnet restore "src/Driftgrid.Api/Driftgrid.Api.csproj"
COPY . .
WORKDIR "/src/src/Driftgrid.Api"
RUN dotnet build "./Driftgrid.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Driftgrid.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Driftgrid.Api.dll"]

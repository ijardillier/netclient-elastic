FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 9091

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src

COPY NetCoreClient.Elk.csproj src/NetCoreClient.Elk/

RUN dotnet restore src/NetCoreClient.Elk/NetCoreClient.Elk.csproj

COPY . .
WORKDIR /src
RUN dotnet publish no-restore -c Debug -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NetCoreClient.Elk.dll"]
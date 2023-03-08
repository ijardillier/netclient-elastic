FROM mcr.microsoft.com/dotnet/core/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 9091

FROM mcr.microsoft.com/dotnet/core/sdk:6.0-alpine AS build
WORKDIR /src

COPY NetClient.Elastic.csproj src/NetClient.Elastic/

RUN dotnet restore src/NetClient.Elastic/NetClient.Elastic.csproj

COPY . .
WORKDIR /src
RUN dotnet publish no-restore -c Debug -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NetClient.Elastic.dll"]
ARG AGENT_VERSION=1.20.0

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.16 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.16 AS build
ARG AGENT_VERSION

# install zip curl
RUN apk update && apk add zip wget

# pull down the zip file based on ${AGENT_VERSION} ARG and unzip
RUN wget -q https://github.com/elastic/apm-agent-dotnet/releases/download/v${AGENT_VERSION}/elastic_apm_profiler_${AGENT_VERSION}-linux-x64.zip && \
    unzip elastic_apm_profiler_${AGENT_VERSION}-linux-x64.zip -d /elastic_apm_profiler

RUN wget -q https://github.com/elastic/apm-agent-dotnet/releases/download/v${AGENT_VERSION}/ElasticApmAgent_${AGENT_VERSION}.zip && \
    unzip ElasticApmAgent_${AGENT_VERSION}.zip -d /ElasticApmAgent

WORKDIR /src

COPY . .

RUN dotnet restore NetClient.Elastic.csproj
RUN dotnet build -c Release -o /app/build

FROM build AS publish

RUN dotnet publish -c Release -o /app/publish

FROM base AS final

WORKDIR /elastic_apm_profiler
COPY --from=publish /elastic_apm_profiler .
WORKDIR /ElasticApmAgent
COPY --from=publish /ElasticApmAgent .

WORKDIR /app
COPY --from=publish /app/publish .

ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={FA65FE15-F085-4681-9B20-95E04F6C03CC}
ENV CORECLR_PROFILER_PATH=/elastic_apm_profiler/libelastic_apm_profiler.so
ENV ELASTIC_APM_PROFILER_HOME=/elastic_apm_profiler
ENV ELASTIC_APM_PROFILER_INTEGRATIONS=/elastic_apm_profiler/integrations.yml
ENV DOTNET_STARTUP_HOOKS=/ElasticApmAgent/ElasticApmAgentStartupHook.dll
ENV ELASTIC_APM_SERVER_URL=https://host.docker.internal:8200
ENV ELASTIC_APM_VERIFY_SERVER_CERT=false
ENV ELASTIC_APM_SERVICE_NAME=NetClient-Elastic
ENV ELASTIC_APM_LOG_LEVEL=Debug
ENV ELASTIC_APM_ENVIRONMENT=Development
ENV ELASTIC_APM_STARTUP_HOOKS_LOGGING=1

ENTRYPOINT ["dotnet", "NetClient.Elastic.dll"]
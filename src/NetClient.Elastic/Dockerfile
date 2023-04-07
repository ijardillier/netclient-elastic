#ARG AGENT_VERSION=1.20.0

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.16 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.16 AS build
#ARG AGENT_VERSION

# # install zip curl
# RUN apk update && apk add zip wget

# pull down the zip file based on ${AGENT_VERSION} ARG and unzip
# RUN wget -q https://github.com/elastic/apm-agent-dotnet/releases/download/v${AGENT_VERSION}/elastic_apm_profiler_${AGENT_VERSION}-linux-x64.zip && \
#     unzip elastic_apm_profiler_${AGENT_VERSION}-linux-x64.zip -d /elastic_apm_profiler

# RUN wget -q https://github.com/elastic/apm-agent-dotnet/releases/download/v${AGENT_VERSION}/ElasticApmAgent_${AGENT_VERSION}.zip && \
#     unzip ElasticApmAgent_${AGENT_VERSION}.zip -d /ElasticApmAgent

WORKDIR /src

COPY . .

RUN dotnet restore NetClient.Elastic.csproj
RUN dotnet build -c Release -o /app/build

FROM build AS publish

RUN dotnet publish -c Release -o /app/publish

FROM base AS final

# WORKDIR /elastic_apm_profiler
# COPY --from=publish /elastic_apm_profiler .
# WORKDIR /ElasticApmAgent
# COPY --from=publish /ElasticApmAgent .

WORKDIR /app
COPY --from=publish /app/publish .

# # Configures whether profiling is enabled for the currently running process.
# ENV CORECLR_ENABLE_PROFILING=1
# # Specifies the GUID of the profiler to load into the currently running process.
# ENV CORECLR_PROFILER={FA65FE15-F085-4681-9B20-95E04F6C03CC}
# # Specifies the path to the profiler DLL to load into the currently running process (or 32-bit or 64-bit process).
# ENV CORECLR_PROFILER_PATH=/elastic_apm_profiler/libelastic_apm_profiler.so

# # Specifies the home directory of the profiler auto instrumentation. 
# ENV ELASTIC_APM_PROFILER_HOME=/elastic_apm_profiler
# # Specifies the path to the integrations.yml file that determines which methods to target for auto instrumentation.
# ENV ELASTIC_APM_PROFILER_INTEGRATIONS=/elastic_apm_profiler/integrations.yml
# # Specifies the log level at which the profiler should log. 
# ENV ELASTIC_APM_PROFILER_LOG=warn

# # Inject the APM agent at startup
# ENV DOTNET_STARTUP_HOOKS=/ElasticApmAgent/ElasticApmAgentStartupHook.dll
# # If the startup hook integration throws an exception, additional detail can be obtained by setting the Startup Hooks Logging variable.
# ENV ELASTIC_APM_STARTUP_HOOKS_LOGGING=1

# # Core configuration options / Specifies the service name (ElasticApm:ServiceName).
# ENV ELASTIC_APM_SERVICE_NAME=NetClient-Elastic
# # Core configuration options / Specifies the environment (ElasticApm:Environment)
# ENV ELASTIC_APM_ENVIRONMENT=Development
# # Core configuration options / Spcifies the sample rate (ElasticApm:TransactionSampleRate).
# # 1.0 : Dev purpose only, should be lowered in Production to reduce overhead.
# ENV ELASTIC_APM_TRANSACTION_SAMPLE_RATE=1.0 

# # Reporter configuration options / Specifies the URL for your APM Server (ElasticApm:ServerUrl).
# ENV ELASTIC_APM_SERVER_URL=https://host.docker.internal:8200
# # Reporter configuration options / Specifies if the agent should verify the SSL certificate if using HTTPS connection to the APM server (ElasticApm:VerifyServerCert). 
# ENV ELASTIC_APM_VERIFY_SERVER_CERT=false
# # Reporter configuration options / Specifies the path to a PEM-encoded certificate used for SSL/TLS by APM server (ElasticApm:ServerCert).
# # ENV ELASTIC_APM_SERVER_CERT=

# # Supportability configuration options / Sets the logging level for the agent (ElasticApm:LogLevel).
# ENV ELASTIC_APM_LOG_LEVEL=Debug

ENTRYPOINT ["dotnet", "NetClient.Elastic.dll"]
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeeNChill.csproj", "./"]
RUN dotnet restore "./CoffeeNChill.csproj"
COPY . .
RUN dotnet build "./CoffeeNChill.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoffeeNChill.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .

ENV AzureWebJobsStorage=UseDevelopmentStorage=true
ENV FUNCTIONS_WORKER_RUNTIME=dotnet-isolated
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true

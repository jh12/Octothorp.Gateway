FROM mcr.microsoft.com/dotnet/core/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster-slim AS publish
ARG RELEASE_VERSION
WORKDIR /src
COPY ["Octothorp.Gateway/Octothorp.Gateway.csproj", "Octothorp.Gateway/"]
RUN dotnet restore "Octothorp.Gateway/Octothorp.Gateway.csproj"
COPY . .
WORKDIR "/src/Octothorp.Gateway"
RUN dotnet publish "Octothorp.Gateway.csproj" -c Release -p:VersionPrefix=$RELEASE_VERSION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Octothorp.Gateway.dll"]
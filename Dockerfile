# ====== Production ====== #
FROM mcr.microsoft.com/dotnet/aspnet:8.0 as base
USER app
WORKDIR /app
EXPOSE 8080

# ====== Build image ====== #
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
ARG RELEASE_VERSION
ARG BUILD_CONFIGURATION=Release
WORKDIR /sln

COPY ./*.sln ./
COPY Octothorp.Gateway src/Octothorp.Gateway
COPY Octothorp.Gateway.Shared src/Octothorp.Gateway.Shared

RUN ls -l .
RUN ls -l src/

RUN dotnet restore "./src/Octothorp.Gateway/Octothorp.Gateway.csproj" -a $TARGETARCH
RUN dotnet restore "./src/Octothorp.Gateway.Shared/Octothorp.Gateway.Shared.csproj" -a $TARGETARCH

RUN dotnet build "./src/Octothorp.Gateway/Octothorp.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -p:VersionPrefix=$RELEASE_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./src/Octothorp.Gateway/Octothorp.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -p:VersionPrefix=$RELEASE_VERSION

# ====== Copy to final ====== #
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Octothorp.Gateway.dll"]
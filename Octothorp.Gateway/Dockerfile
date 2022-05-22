FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS publish
ARG RELEASE_VERSION
WORKDIR /src
COPY ["Octothorp.Gateway/Octothorp.Gateway.csproj", "Octothorp.Gateway/"]
RUN dotnet restore "Octothorp.Gateway/Octothorp.Gateway.csproj"
COPY . .
WORKDIR "/src/Octothorp.Gateway"
RUN dotnet publish "Octothorp.Gateway.csproj" -c Release -p:VersionPrefix=$RELEASE_VERSION -o /app/publish

FROM base AS final

RUN addgroup --system --gid 1000 netcoregroup \
&& adduser --system --uid 1000 --ingroup netcoregroup --shell /bin/sh netcoreuser

WORKDIR /app
COPY --from=publish /app/publish .

USER 1000
ENTRYPOINT ["dotnet", "Octothorp.Gateway.dll"]
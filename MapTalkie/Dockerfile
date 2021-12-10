FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MapTalkie", "MapTalkie"]
COPY ["MapTalkie.DB", "MapTalkie.DB"]
COPY ["MapTalkie.Domain", "MapTalkie.Domain"]
RUN dotnet restore "./MapTalkie/MapTalkie.csproj"

WORKDIR /src/MapTalkie
RUN mkdir -p /app/build
RUN dotnet build "MapTalkie.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapTalkie.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MapTalkie.dll"]

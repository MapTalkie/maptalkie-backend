FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MapTalkie.Domain", "MapTalkie.Domain"]
COPY ["MapTalkie.DB", "MapTalkie.DB"]
COPY ["MapTalkie.Services.Posts", "MapTalkie.Services.Posts"]
RUN dotnet restore "MapTalkie.Services.Posts/MapTalkie.Services.Posts.csproj"

WORKDIR "/src/MapTalkie.Services.Posts"
RUN dotnet build "MapTalkie.Services.Posts.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapTalkie.Services.Posts.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MapTalkie.Services.Posts.dll"]

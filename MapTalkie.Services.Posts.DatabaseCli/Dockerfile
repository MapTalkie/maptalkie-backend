FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MapTalkie.Services.Posts.DatabaseCli/MapTalkie.Services.Posts.DatabaseCli.csproj", "MapTalkie.Services.Posts.DatabaseCli/"]
RUN dotnet restore "MapTalkie.Services.Posts.DatabaseCli/MapTalkie.Services.Posts.DatabaseCli.csproj"
COPY . .
WORKDIR "/src/MapTalkie.Services.Posts.DatabaseCli"
RUN dotnet build "MapTalkie.Services.Posts.DatabaseCli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapTalkie.Services.Posts.DatabaseCli.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MapTalkie.Services.Posts.DatabaseCli.dll"]

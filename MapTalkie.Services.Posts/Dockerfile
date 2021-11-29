FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ./MapTalkie.Services.Posts/appsettings* MapTalkie.Services.Posts/

COPY ./MapTalkie.Services.Posts/MapTalkie.Services.Posts.csproj MapTalkie.Services.Posts/
COPY ./MapTalkie.Services.Posts.Messages/MapTalkie.Services.Posts.Messages.csproj MapTalkie.Services.Posts.Messages/
RUN dotnet restore "MapTalkie.Services.Posts/MapTalkie.Services.Posts.csproj"
RUN dotnet restore "MapTalkie.Services.Posts.Messages/MapTalkie.Services.Posts.Messages.csproj"
COPY ./MapTalkie.Services.Posts ./MapTalkie.Services.Posts
COPY ./MapTalkie.Services.Posts.Messages ./MapTalkie.Services.Posts.Messages
WORKDIR "/src/MapTalkie.Services.Posts"
RUN dotnet build "MapTalkie.Services.Posts.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapTalkie.Services.Posts.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "MapTalkie.Services.Posts.dll"]

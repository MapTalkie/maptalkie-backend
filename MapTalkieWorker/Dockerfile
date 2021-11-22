FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MapTalkieWorker/MapTalkieWorker.csproj", "MapTalkieWorker/"]
RUN dotnet restore "MapTalkieWorker/MapTalkieWorker.csproj"
COPY . .
WORKDIR "/src/MapTalkieWorker"
RUN dotnet build "MapTalkieWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapTalkieWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MapTalkieWorker.dll"]

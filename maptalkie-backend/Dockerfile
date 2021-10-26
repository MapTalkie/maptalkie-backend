FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["maptalkie-backend/maptalkie-backend.csproj", "maptalkie-backend/"]
RUN dotnet restore "maptalkie-backend/maptalkie-backend.csproj"
COPY . .
WORKDIR "/src/maptalkie-backend"
RUN dotnet build "maptalkie-backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "maptalkie-backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "maptalkie-backend.dll"]

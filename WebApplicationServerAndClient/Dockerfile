# 1. Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApplicationServerAndClient/WebApplicationServerAndClient.csproj", "WebApplicationServerAndClient/"]
RUN dotnet restore "./WebApplicationServerAndClient/WebApplicationServerAndClient.csproj"
COPY . .
WORKDIR "/src/WebApplicationServerAndClient"
RUN dotnet build "./WebApplicationServerAndClient.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 2. Этап публикации
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebApplicationServerAndClient.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 3. Финальная стадия - используем образ aspnet
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# Копируем wait-for-it.sh в контейнер
COPY WebApplicationServerAndClient/wait-for-it.sh /usr/local/bin/wait-for-it
RUN chmod +x /usr/local/bin/wait-for-it

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["wait-for-it", "db:5432", "--", "dotnet", "WebApplicationServerAndClient.dll"]

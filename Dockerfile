FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["SpaceChatServer.csproj", "./"]
RUN dotnet restore "SpaceChatServer.csproj" --source https://mirrors.huaweicloud.com/repository/nuget/v3/index.json

COPY . .
RUN dotnet publish "SpaceChatServer.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

RUN apk add --no-cache icu-libs icu-data-full

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LANG=ru_RU.UTF-8
ENV LC_ALL=ru_RU.UTF-8

ENTRYPOINT ["dotnet", "SpaceChatServer.dll"]
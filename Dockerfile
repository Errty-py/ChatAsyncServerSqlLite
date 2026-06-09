FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /app

COPY SpaceChatServer.csproj ./

RUN dotnet restore SpaceChatServer.csproj

COPY . .

RUN dotnet publish SpaceChatServer.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8000

ENTRYPOINT ["dotnet", "SpaceChatServer.dll"]
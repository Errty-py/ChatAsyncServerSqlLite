FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish SpaceChatServer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 as final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SpaceChatServer.dll"]
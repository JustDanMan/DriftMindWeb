FROM mcr.microsoft.com/dotnet/aspnet:8.0-azurelinux-distroless AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0-azurelinux AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish -p:DebugType=None -p:DebugSymbols=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DriftMindWeb.dll"]

# syntax=docker/dockerfile:1
# Imagem de producao da API UniHub (.NET 10) para deploy no Render.
# Multi-stage: SDK compila; runtime enxuto roda (plano free: 512MB/0.1 CPU).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Backend.csproj ./
RUN dotnet restore Backend.csproj
COPY . ./
RUN dotnet publish Backend.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish ./
EXPOSE 10000
ENTRYPOINT ["dotnet", "Backend.dll"]

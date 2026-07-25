# Stage 1 - restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY *.sln ./
COPY src/SP.Domain/SP.Domain.csproj src/SP.Domain/
COPY src/SP.Application/SP.Application.csproj src/SP.Application/
COPY src/SP.Infrastructure/SP.Infrastructure.csproj src/SP.Infrastructure/
COPY src/SP.API/SP.API.csproj src/SP.API/

RUN dotnet restore src/SP.API/SP.API.csproj

COPY . .

# Stage 2 - build and publish
FROM restore AS publish

RUN dotnet publish src/SP.API/SP.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 3 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=20s --timeout=10s --start-period=60s --retries=5 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SP.API.dll"]

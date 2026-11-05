# Multi-stage build for .NET 8 backend

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["src/backend/HouseDemo.Backend/HouseDemo.Backend.csproj", "HouseDemo.Backend/"]

# Restore NuGet packages
RUN dotnet restore "HouseDemo.Backend/HouseDemo.Backend.csproj"

# Copy source code
COPY ["src/backend/HouseDemo.Backend/", "HouseDemo.Backend/"]

# Build application
WORKDIR "/src/HouseDemo.Backend"
RUN dotnet build "HouseDemo.Backend.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "HouseDemo.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment
ENV ASPNETCORE_URLS=http://+:8080

# Run application
ENTRYPOINT ["dotnet", "HouseDemo.Backend.dll"]

# Use official .NET 8 runtime as base image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the entire backend solution
COPY NotiBlock.Backend/ ./

# Restore dependencies
RUN dotnet restore "NotiBlock.Backend.csproj"

# Build the application
RUN dotnet build "NotiBlock.Backend.csproj" -c Release -o /app/build --no-restore

# Publish the application
FROM build AS publish
RUN dotnet publish "NotiBlock.Backend.csproj" -c Release -o /app/publish --no-build

# Use official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install ca-certificates for HTTPS
RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Expose ports (7179 for HTTPS, 5271 for HTTP)
EXPOSE 5271 7179

# Set environment variables for .NET
ENV ASPNETCORE_URLS=http://+:5271;https://+:7179
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=""
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=""

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5271/api/auth/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "NotiBlock.Backend.dll"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["thuctapCN/thuctapCN.csproj", "thuctapCN/"]
RUN dotnet restore "thuctapCN/thuctapCN.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/thuctapCN"
RUN dotnet build "thuctapCN.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "thuctapCN.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads/avatars

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:5000/ || exit 1

# Run the app
ENTRYPOINT ["dotnet", "thuctapCN.dll"]


# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["TRRCMS.sln", "./"]

# Copy project files for restore (improves layer caching)
COPY ["src/TRRCMS.Domain/TRRCMS.Domain.csproj", "src/TRRCMS.Domain/"]
COPY ["src/TRRCMS.Application/TRRCMS.Application.csproj", "src/TRRCMS.Application/"]
COPY ["src/TRRCMS.Infrastructure/TRRCMS.Infrastructure.csproj", "src/TRRCMS.Infrastructure/"]
COPY ["src/TRRCMS.WebAPI/TRRCMS.WebAPI.csproj", "src/TRRCMS.WebAPI/"]

# Restore dependencies
RUN dotnet restore "src/TRRCMS.WebAPI/TRRCMS.WebAPI.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/src/TRRCMS.WebAPI"
RUN dotnet publish "TRRCMS.WebAPI.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set working directory
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published output from build stage
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port 8080 (non-privileged port)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "TRRCMS.WebAPI.dll"]
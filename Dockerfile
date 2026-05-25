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

# Install curl for health checks and debugging
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security with a PINNED uid/gid (1001).
# The uid must be fixed: named volumes (packages/uploads/archives) persist the
# ownership they were first created with. With an unpinned `useradd -r`, the uid
# can change between image rebuilds, leaving previously-created volumes owned by
# a uid that no longer matches appuser — which makes writes fail with
# "Access to the path ... is denied". A fixed uid keeps volume ownership stable
# across rebuilds so a freshly created volume (after `docker compose down -v`)
# inherits appuser ownership from the chowned image dirs below and stays writable.
RUN groupadd -r -g 1001 appuser && useradd -r -u 1001 -g appuser appuser

# Copy published output from build stage
COPY --from=build /app/publish .

# Pre-create runtime directories that are mounted as Docker volumes.
# Must happen before USER appuser so we can chown them — Docker mounts
# volumes after the container starts. A named volume inherits the owner of
# the image directory at its mount path; if that directory does NOT exist in
# the image, Docker creates the mountpoint as root:root and the non-root
# appuser cannot write to it. /app/wwwroot/uploads (extracted import
# attachments, building/ID documents, survey uploads) must be listed here for
# the same reason as packages/archives — otherwise package file extraction
# fails silently and committed document rows point at files that were never
# written.
RUN mkdir -p /app/wwwroot/packages /app/wwwroot/uploads /app/archives \
    && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port 8080 (non-privileged port)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development} \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "TRRCMS.WebAPI.dll"]

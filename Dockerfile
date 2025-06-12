# GitChangesAnalyzer Dockerfile
# Multi-stage build for optimized production image

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["GitChangesAnalyzer.csproj", "."]
COPY ["tests/GitChangesAnalyzer.Tests.csproj", "tests/"]
COPY ["Directory.Build.props", "."]

# Restore dependencies
RUN dotnet restore "GitChangesAnalyzer.csproj"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "GitChangesAnalyzer.csproj" -c Release -o /app/build

# Test stage (optional - can be skipped in production builds)
FROM build AS test
RUN dotnet test "tests/GitChangesAnalyzer.Tests.csproj" --configuration Release --verbosity normal

# Publish stage
FROM build AS publish
RUN dotnet publish "GitChangesAnalyzer.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Install git (required for the application to work)
RUN apt-get update && \
    apt-get install -y git && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r gitanalyzer && useradd -r -g gitanalyzer gitanalyzer

# Copy published application
COPY --from=publish /app/publish .

# Create output directory and set permissions
RUN mkdir -p /app/output && \
    chown -R gitanalyzer:gitanalyzer /app

# Switch to non-root user
USER gitanalyzer

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Expose volumes for git repositories and output
VOLUME ["/workspace", "/app/output"]

# Default working directory for git operations
WORKDIR /workspace

# Entry point
ENTRYPOINT ["dotnet", "/app/GitChangesAnalyzer.dll"]

# Default command (show help)
CMD ["--help"]

# Docker Deployment Guide

This document explains how to use GitChangesAnalyzer as a Docker container.

## 🐳 Docker Images

The project provides three Docker image variants:

| Image | Purpose | Size | Use Case |
|-------|---------|------|----------|
| `gitchangesanalyzer:latest` | Production runtime | ~200MB | Running analysis in production |
| `gitchangesanalyzer:dev` | Development | ~1GB | Development and debugging |
| `gitchangesanalyzer:test` | Testing | ~1GB | Running tests in CI/CD |

## 🚀 Quick Start

### 1. Build the Docker Image

```bash
# Build production image
./scripts/docker-build.sh

# Or using make
make docker-build

# Build development image
./scripts/docker-build.sh --dev
```

### 2. Run Analysis

```bash
# Analyze current repository
./scripts/docker-run.sh --commit abc123

# Analyze different repository
./scripts/docker-run.sh --repo /path/to/repo --commit abc123 --verbose

# Using docker directly
docker run --rm \
  -v "$(pwd):/workspace:ro" \
  -v "$(pwd)/output:/app/output" \
  gitchangesanalyzer:latest \
  --commit abc123 --verbose
```

## 📋 Docker Commands Reference

### Building Images

```bash
# Production image (optimized, smallest size)
docker build --target final --tag gitchangesanalyzer:latest .

# Development image (includes SDK and tools)
docker build --target build --tag gitchangesanalyzer:dev .

# Test image (runs tests during build)
docker build --target test --tag gitchangesanalyzer:test .
```

### Running Containers

#### Basic Usage
```bash
# Show help
docker run --rm gitchangesanalyzer:latest --help

# Analyze specific commit
docker run --rm \
  -v "/path/to/repo:/workspace:ro" \
  -v "/path/to/output:/app/output" \
  gitchangesanalyzer:latest \
  --commit abc123
```

#### Advanced Usage
```bash
# XML-focused analysis
docker run --rm \
  -v "$(pwd):/workspace:ro" \
  -v "$(pwd)/output:/app/output" \
  gitchangesanalyzer:latest \
  --commit abc123 --include "*.xml" --xml-mode

# Multi-commit range analysis
docker run --rm \
  -v "$(pwd):/workspace:ro" \
  -v "$(pwd)/output:/app/output" \
  gitchangesanalyzer:latest \
  --commit-range abc123..def456 --verbose
```

## 🔧 Docker Compose

### Production Deployment

```yaml
# docker-compose.yml
version: '3.8'
services:
  analyzer:
    image: gitchangesanalyzer:latest
    volumes:
      - ${GIT_REPO_PATH}:/workspace:ro
      - ${OUTPUT_PATH}:/app/output
    environment:
      - DOTNET_ENVIRONMENT=Production
    command: ["--commit", "${COMMIT_HASH}", "--verbose"]
```

```bash
# Set environment variables
export GIT_REPO_PATH="/path/to/repo"
export OUTPUT_PATH="/path/to/output"
export COMMIT_HASH="abc123"

# Run analysis
docker-compose up analyzer
```

### Development Setup

```bash
# Start development container
docker-compose up -d gitchangesanalyzer-dev

# Connect to development container
docker exec -it gitchangesanalyzer-dev bash

# Inside container
dotnet build
dotnet test
dotnet run -- --help
```

## 💾 Volume Mounts

| Volume | Purpose | Access |
|--------|---------|--------|
| `/workspace` | Git repository to analyze | Read-only |
| `/app/output` | Generated analysis reports | Read-write |

### Example Volume Configurations

```bash
# Current directory as repo, output to ./output
-v "$(pwd):/workspace:ro" -v "$(pwd)/output:/app/output"

# Different repo and output paths
-v "/home/user/project:/workspace:ro" -v "/tmp/analysis:/app/output"

# Windows paths (Git Bash/WSL)
-v "/c/Users/user/project:/workspace:ro" -v "/c/temp/output:/app/output"
```

## 🔒 Security Considerations

### Non-Root User
The container runs as a non-root user (`gitanalyzer`) for security:

```dockerfile
RUN groupadd -r gitanalyzer && useradd -r -g gitanalyzer gitanalyzer
USER gitanalyzer
```

### Read-Only Repository
Mount the repository as read-only to prevent accidental modifications:

```bash
-v "/path/to/repo:/workspace:ro"  # Note the :ro suffix
```

### Resource Limits
Set memory and CPU limits for production deployments:

```bash
docker run --rm \
  --memory=1g \
  --cpus=1.0 \
  gitchangesanalyzer:latest \
  --commit abc123
```

## 🚀 CI/CD Integration

### GitHub Actions

```yaml
name: Git Analysis
on: [push]

jobs:
  analyze:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Full history for multi-commit analysis
      
      - name: Build Analyzer
        run: docker build -t gitchangesanalyzer .
      
      - name: Analyze Changes
        run: |
          docker run --rm \
            -v "${{ github.workspace }}:/workspace:ro" \
            -v "${{ github.workspace }}/reports:/app/output" \
            gitchangesanalyzer \
            --commit "${{ github.sha }}" --verbose
      
      - name: Upload Analysis
        uses: actions/upload-artifact@v4
        with:
          name: git-analysis-report
          path: reports/
```

### GitLab CI

```yaml
analyze-changes:
  stage: analyze
  image: docker:latest
  services:
    - docker:dind
  script:
    - docker build -t gitchangesanalyzer .
    - mkdir -p reports
    - docker run --rm 
        -v "$PWD:/workspace:ro" 
        -v "$PWD/reports:/app/output" 
        gitchangesanalyzer 
        --commit $CI_COMMIT_SHA --verbose
  artifacts:
    paths:
      - reports/
    expire_in: 1 week
```

## 🐛 Troubleshooting

### Common Issues

1. **Permission Denied**
   ```bash
   # Fix output directory permissions
   sudo chown -R $USER:$USER ./output
   ```

2. **Path Not Found (Windows)**
   ```bash
   # Use Unix-style paths in Git Bash/WSL
   docker run -v "/c/path/to/repo:/workspace:ro" ...
   ```

3. **Git Repository Not Found**
   ```bash
   # Ensure .git directory exists in mounted path
   ls -la /path/to/repo/.git
   ```

4. **Container Exits Immediately**
   ```bash
   # Check container logs
   docker logs gitchangesanalyzer
   
   # Run with shell for debugging
   docker run --rm -it --entrypoint /bin/bash gitchangesanalyzer:latest
   ```

### Debug Mode

```bash
# Run development container for debugging
docker run --rm -it \
  -v "$(pwd):/workspace:ro" \
  -v "$(pwd)/output:/app/output" \
  --entrypoint /bin/bash \
  gitchangesanalyzer:dev

# Inside container
cd /src
dotnet run -- --help
```

## 📊 Performance

### Image Sizes
- **Production**: ~200MB (runtime + app)
- **Development**: ~1GB (SDK + tools)
- **Alpine-based**: ~150MB (smaller runtime)

### Resource Usage
- **Memory**: 100-500MB typical
- **CPU**: 1 core sufficient for most analyses
- **Storage**: 50-100MB per analysis report

## 🔄 Updates

### Updating Images

```bash
# Rebuild with latest changes
docker build --no-cache --tag gitchangesanalyzer:latest .

# Pull base image updates
docker pull mcr.microsoft.com/dotnet/runtime:8.0
docker build --tag gitchangesanalyzer:latest .
```

### Version Management

```bash
# Tag specific versions
docker build --tag gitchangesanalyzer:v1.0.0 .
docker build --tag gitchangesanalyzer:latest .

# Push to registry
docker tag gitchangesanalyzer:latest your-registry/gitchangesanalyzer:latest
docker push your-registry/gitchangesanalyzer:latest
```

# GitHub Actions Docker Hub Publishing Guide

This guide explains how to set up automatic Docker image publishing to Docker Hub using GitHub Actions.

## 🔧 Setup Requirements

### 1. Docker Hub Account
- Create a [Docker Hub](https://hub.docker.com) account if you don't have one
- Create a repository for your image (e.g., `yourusername/gitchangesanalyzer`)

### 2. Docker Hub Access Token
1. Go to Docker Hub → Account Settings → Security
2. Click "New Access Token"
3. Name: `github-actions` (or similar)
4. Permissions: Read, Write, Delete
5. Copy the generated token (you won't see it again!)

### 3. GitHub Repository Secrets
Add these secrets to your GitHub repository:

1. Go to your GitHub repository
2. Settings → Secrets and variables → Actions
3. Add these Repository Secrets:

| Secret Name | Value | Description |
|-------------|-------|-------------|
| `DOCKERHUB_USERNAME` | `yourusername` | Your Docker Hub username |
| `DOCKERHUB_TOKEN` | `dckr_pat_...` | Your Docker Hub access token |

## 🚀 Workflow Overview

The workflow (`.github/workflows/docker.yml`) does the following:

### Triggers
- **Push to main/master**: Builds and pushes `latest` and `dev` images
- **Git tags (v*)**: Builds and pushes versioned releases (e.g., `v1.0.0`)
- **Pull requests**: Builds only (no push) for testing
- **Releases**: Builds and pushes release images

### Generated Images

| Tag | When | Description |
|-----|------|-------------|
| `latest` | main/master push | Latest stable build |
| `dev` | main/master push | Development build with SDK |
| `v1.0.0` | Git tag `v1.0.0` | Specific version |
| `1.0` | Git tag `v1.0.0` | Major.minor version |
| `1` | Git tag `v1.0.0` | Major version |

### Multi-Architecture Support
- `linux/amd64` (Intel/AMD 64-bit)
- `linux/arm64` (ARM 64-bit, Apple Silicon, etc.)

## 📋 Step-by-Step Setup

### Step 1: Add Secrets to GitHub

```bash
# Navigate to your GitHub repository
# Go to: Settings → Secrets and variables → Actions
# Click "New repository secret" and add:

Name: DOCKERHUB_USERNAME
Value: your-dockerhub-username

Name: DOCKERHUB_TOKEN  
Value: your-dockerhub-access-token
```

### Step 2: Update Image Name (Optional)

Edit `.github/workflows/docker.yml` if you want a different image name:

```yaml
env:
  REGISTRY: docker.io
  IMAGE_NAME: your-preferred-name  # Change this
```

### Step 3: Test the Workflow

1. **Push to main branch**:
   ```bash
   git push origin main
   ```

2. **Create a release tag**:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. **Check workflow status**:
   - Go to Actions tab in your GitHub repository
   - Monitor the "Build and Publish Docker Image" workflow

## 🔍 Monitoring and Troubleshooting

### Check Workflow Status
1. GitHub repository → Actions tab
2. Click on the latest workflow run
3. Expand job steps to see detailed logs

### Common Issues

#### ❌ Authentication Failed
```
Error: unauthorized: authentication required
```
**Solution**: Check that `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` secrets are set correctly.

#### ❌ Repository Not Found
```
Error: repository does not exist or may require 'docker login'
```
**Solutions**:
- Ensure the Docker Hub repository exists
- Check the image name in the workflow matches your Docker Hub repo
- Verify your Docker Hub username is correct

#### ❌ Token Permissions
```
Error: insufficient privileges
```
**Solution**: Ensure your Docker Hub token has Read, Write, Delete permissions.

### Debug Steps

1. **Check secrets**:
   ```bash
   # In GitHub: Settings → Secrets → Actions
   # Verify DOCKERHUB_USERNAME and DOCKERHUB_TOKEN exist
   ```

2. **Test Docker Hub access locally**:
   ```bash
   docker login
   docker tag gitchangesanalyzer:latest yourusername/gitchangesanalyzer:test
   docker push yourusername/gitchangesanalyzer:test
   ```

3. **Check workflow logs**:
   - Look for the "Log in to Docker Hub" step
   - Check the "Build and push Docker image" step output

## 🎯 Usage After Setup

Once set up, your Docker images will be automatically published:

### Pull Images
```bash
# Latest stable version
docker pull yourusername/gitchangesanalyzer:latest

# Development version  
docker pull yourusername/gitchangesanalyzer:dev

# Specific version
docker pull yourusername/gitchangesanalyzer:v1.0.0
```

### Run from Docker Hub
```bash
# Run latest version
docker run --rm \
  -v "$(pwd):/workspace:ro" \
  -v "$(pwd)/output:/app/output" \
  yourusername/gitchangesanalyzer:latest \
  --commit abc123 --verbose
```

## 🔐 Security Features

### Vulnerability Scanning
The workflow includes Trivy security scanning:
- Scans for critical and high vulnerabilities
- Results appear in GitHub Security tab
- Runs on every push to main/master

### Multi-Stage Builds
- Production image is minimal (runtime only)
- Development image includes SDK for debugging
- Separate stages for testing and production

### Non-Root User
- Images run as non-root user `gitanalyzer`
- Improved security posture
- Follows Docker best practices

## 📊 Workflow Matrix (Advanced)

For multiple .NET versions or additional testing:

```yaml
strategy:
  matrix:
    dotnet-version: ['8.0.x']
    os: [ubuntu-latest]
    include:
      - dotnet-version: '9.0.x'
        os: ubuntu-latest
        experimental: true
```

## 🏷️ Tagging Strategy

### Semantic Versioning
```bash
# Major release
git tag v2.0.0

# Minor release  
git tag v1.1.0

# Patch release
git tag v1.0.1

# Pre-release
git tag v1.1.0-beta.1
```

### Automatic Tags
The workflow automatically creates:
- `latest` for main branch
- `v1.0.0`, `1.0`, `1` for tag `v1.0.0`
- `dev`, `latest-dev` for development builds

## 🔄 Workflow Customization

### Build Only on Tags
To publish only on releases:

```yaml
on:
  push:
    tags: [ 'v*' ]
  release:
    types: [ published ]
```

### Different Registries
To use GitHub Container Registry instead:

```yaml
env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}
```

### Custom Build Args
To pass build arguments:

```yaml
- name: Build and push Docker image
  uses: docker/build-push-action@v5
  with:
    build-args: |
      VERSION=${{ github.ref_name }}
      BUILD_DATE=${{ steps.date.outputs.date }}
```

## 📈 Monitoring Usage

### Docker Hub Analytics
- View pull statistics in Docker Hub
- Monitor download trends
- Track popular tags

### GitHub Insights
- Actions usage and billing
- Workflow run history
- Security alerts

This setup provides a robust, automated Docker publishing pipeline that follows best practices for security, versioning, and multi-architecture support.

#!/bin/bash
# Docker build script for GitChangesAnalyzer

set -e

echo "Building GitChangesAnalyzer Docker image..."

# Parse command line arguments
BUILD_TARGET="final"
IMAGE_TAG="gitchangesanalyzer:latest"
NO_CACHE=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --dev)
            BUILD_TARGET="build"
            IMAGE_TAG="gitchangesanalyzer:dev"
            shift
            ;;
        --test)
            BUILD_TARGET="test"
            IMAGE_TAG="gitchangesanalyzer:test"
            shift
            ;;
        --no-cache)
            NO_CACHE="--no-cache"
            shift
            ;;
        --tag)
            IMAGE_TAG="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  --dev       Build development image"
            echo "  --test      Build test image"
            echo "  --no-cache  Build without using cache"
            echo "  --tag TAG   Specify custom image tag"
            echo "  -h, --help  Show this help"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "Building target: $BUILD_TARGET"
echo "Image tag: $IMAGE_TAG"

# Build the Docker image
docker build \
    --target "$BUILD_TARGET" \
    --tag "$IMAGE_TAG" \
    $NO_CACHE \
    .

if [ $? -eq 0 ]; then
    echo "✅ Docker image built successfully: $IMAGE_TAG"
    
    # Show image size
    docker images "$IMAGE_TAG" --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"
    
    echo ""
    echo "Usage examples:"
    echo "  docker run --rm $IMAGE_TAG --help"
    echo "  docker run --rm -v \"\$(pwd):/workspace\" -v \"\$(pwd)/output:/app/output\" $IMAGE_TAG --commit abc123"
    echo ""
    echo "Or use docker-compose:"
    echo "  docker-compose up gitchangesanalyzer"
else
    echo "❌ Docker build failed"
    exit 1
fi

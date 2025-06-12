#!/bin/bash
# Docker run script for GitChangesAnalyzer

set -e

# Default values
REPO_PATH="$(pwd)"
OUTPUT_PATH="$(pwd)/output"
IMAGE_TAG="gitchangesanalyzer:latest"
COMMIT_HASH=""
EXTRA_ARGS=""

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --repo)
            REPO_PATH="$2"
            shift 2
            ;;
        --output)
            OUTPUT_PATH="$2"
            shift 2
            ;;
        --image)
            IMAGE_TAG="$2"
            shift 2
            ;;
        --commit)
            COMMIT_HASH="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS] [ANALYZER_ARGS]"
            echo "Docker run wrapper for GitChangesAnalyzer"
            echo ""
            echo "Options:"
            echo "  --repo PATH     Path to git repository (default: current directory)"
            echo "  --output PATH   Path for output files (default: ./output)"
            echo "  --image TAG     Docker image tag (default: gitchangesanalyzer:latest)"
            echo "  --commit HASH   Git commit hash to analyze"
            echo "  -h, --help      Show this help"
            echo ""
            echo "Any additional arguments are passed to GitChangesAnalyzer"
            echo ""
            echo "Examples:"
            echo "  $0 --commit abc123 --verbose"
            echo "  $0 --repo /path/to/repo --commit abc123..def456"
            echo "  $0 --help  # Show GitChangesAnalyzer help"
            exit 0
            ;;
        *)
            # Collect remaining arguments for the analyzer
            EXTRA_ARGS="$EXTRA_ARGS $1"
            shift
            ;;
    esac
done

# Validate inputs
if [ ! -d "$REPO_PATH" ]; then
    echo "❌ Repository path does not exist: $REPO_PATH"
    exit 1
fi

if [ ! -d "$REPO_PATH/.git" ]; then
    echo "❌ Not a git repository: $REPO_PATH"
    exit 1
fi

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_PATH"

# Build analyzer arguments
ANALYZER_ARGS=""
if [ -n "$COMMIT_HASH" ]; then
    ANALYZER_ARGS="--commit $COMMIT_HASH"
fi
ANALYZER_ARGS="$ANALYZER_ARGS $EXTRA_ARGS"

# Convert Windows paths to Unix paths if needed (for WSL/Git Bash)
REPO_PATH=$(realpath "$REPO_PATH")
OUTPUT_PATH=$(realpath "$OUTPUT_PATH")

echo "Running GitChangesAnalyzer in Docker..."
echo "Repository: $REPO_PATH"
echo "Output: $OUTPUT_PATH"
echo "Image: $IMAGE_TAG"
echo "Arguments: $ANALYZER_ARGS"
echo ""

# Run the Docker container
docker run --rm \
    -v "$REPO_PATH:/workspace:ro" \
    -v "$OUTPUT_PATH:/app/output" \
    "$IMAGE_TAG" \
    $ANALYZER_ARGS

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Analysis complete! Check output directory: $OUTPUT_PATH"
    ls -la "$OUTPUT_PATH"
else
    echo "❌ Analysis failed"
    exit 1
fi

#!/bin/bash
# Setup script for GitChangesAnalyzer development environment

echo "Setting up GitChangesAnalyzer development environment..."

# Check prerequisites
echo "Checking prerequisites..."

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8.0 SDK or later."
    exit 1
else
    dotnet_version=$(dotnet --version)
    echo "✅ .NET SDK found: $dotnet_version"
fi

# Check Git
if ! command -v git &> /dev/null; then
    echo "❌ Git not found. Please install Git."
    exit 1
else
    git_version=$(git --version)
    echo "✅ Git found: $git_version"
fi

# Setup project
echo "Setting up project..."

# Restore dependencies
echo "Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Failed to restore packages"
    exit 1
fi

# Build project
echo "Building project..."
dotnet build --configuration Debug
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

# Run tests
echo "Running tests..."
dotnet test --configuration Debug --verbosity normal
if [ $? -ne 0 ]; then
    echo "⚠️ Some tests failed, but continuing setup..."
fi

# Create output directory if it doesn't exist
if [ ! -d "output" ]; then
    echo "Creating output directory..."
    mkdir -p output
fi

# Make scripts executable
echo "Setting script permissions..."
chmod +x scripts/*.sh 2>/dev/null || true

echo ""
echo "✅ Setup complete!"
echo ""
echo "Next steps:"
echo "1. Try analyzing a commit: ./scripts/Analyze-Commit.ps1 <commit-hash>"
echo "2. Or use directly: dotnet run -- --commit <commit-hash>"
echo "3. See README.md for more usage examples"
echo ""
echo "Development commands:"
echo "- Build: ./scripts/build.sh"
echo "- Clean: ./scripts/clean.sh"
echo "- Test: dotnet test"

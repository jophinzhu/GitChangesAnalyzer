#!/bin/bash

# Build script for GitChangesAnalyzer
echo "Building GitChangesAnalyzer..."

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "Failed to restore dependencies"
    exit 1
fi

# Build the project
echo "Building project..."
dotnet build --configuration Release --no-restore

if [ $? -ne 0 ]; then
    echo "Build failed"
    exit 1
fi

# Run tests
echo "Running tests..."
dotnet test --configuration Release --no-build --verbosity normal

if [ $? -ne 0 ]; then
    echo "Tests failed"
    exit 1
fi

echo "Build completed successfully!"

# Optional: Create a publish build
read -p "Create publish build? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Publishing..."
    dotnet publish --configuration Release --no-build --output ./publish
    echo "Published to ./publish/"
fi

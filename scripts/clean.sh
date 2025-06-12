#!/bin/bash
# Clean build artifacts and reset the project to a clean state

echo "Cleaning GitChangesAnalyzer project..."

# Remove build artifacts
echo "Removing build artifacts..."
rm -rf bin/
rm -rf obj/
rm -rf publish/
rm -rf tests/bin/
rm -rf tests/obj/

# Remove temporary files
echo "Removing temporary files..."
find . -name "*.tmp" -delete 2>/dev/null || true
find . -name "*.temp" -delete 2>/dev/null || true
find . -name "*~" -delete 2>/dev/null || true

# Remove IDE-specific files that might have been created
echo "Removing IDE files..."
rm -rf .vs/ 2>/dev/null || true
rm -rf .vscode/settings.json 2>/dev/null || true
rm -rf .idea/ 2>/dev/null || true

# Clean NuGet cache (optional)
read -p "Clean NuGet package cache? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Cleaning NuGet cache..."
    dotnet nuget locals all --clear
fi

echo "Project cleaned successfully!"
echo "You can now run 'dotnet restore' and 'dotnet build' to rebuild the project."

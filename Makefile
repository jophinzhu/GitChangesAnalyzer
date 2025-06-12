.PHONY: build test clean restore publish help

# Default target
help:
	@echo "Available targets:"
	@echo "  build     - Build the project"
	@echo "  test      - Run tests"
	@echo "  clean     - Clean build artifacts"
	@echo "  restore   - Restore dependencies"
	@echo "  publish   - Create publish build"
	@echo "  help      - Show this help"

# Restore dependencies
restore:
	dotnet restore

# Build the project
build: restore
	dotnet build --configuration Release --no-restore

# Run tests
test: build
	dotnet test --configuration Release --no-build --verbosity normal

# Clean build artifacts
clean:
	dotnet clean
	rm -rf bin/ obj/ publish/ output/

# Create publish build
publish: build
	dotnet publish --configuration Release --no-build --output ./publish

# Build and run a commit analysis example
example: build
	@echo "Example usage (replace with actual commit hash):"
	@echo "dotnet run -- --commit abc123 --verbose"

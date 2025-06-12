.PHONY: build test clean restore publish setup docker-build docker-run docker-test help

# Default target
help:
	@echo "Available targets:"
	@echo "  setup        - Setup development environment"
	@echo "  build        - Build the project"
	@echo "  test         - Run tests"
	@echo "  clean        - Clean build artifacts"
	@echo "  restore      - Restore dependencies"
	@echo "  publish      - Create publish build"
	@echo "  docker-build - Build Docker image"
	@echo "  docker-run   - Run in Docker container"
	@echo "  docker-test  - Run tests in Docker"
	@echo "  help         - Show this help"

# Setup development environment
setup:
	@echo "Setting up development environment..."
	@./scripts/setup.sh || ./scripts/setup.bat

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
	@echo "Cleaning build artifacts..."
	@./scripts/clean.sh || ./scripts/clean.bat

# Create publish build
publish: build
	dotnet publish --configuration Release --no-build --output ./publish

# Build and run a commit analysis example
example: build
	@echo "Example usage (replace with actual commit hash):"
	@echo "dotnet run -- --commit abc123 --verbose"

# Docker targets
docker-build:
	@echo "Building Docker image..."
	@./scripts/docker-build.sh || ./scripts/docker-build.bat

docker-run:
	@echo "Running GitChangesAnalyzer in Docker..."
	@echo "Usage: make docker-run COMMIT=abc123"
	@./scripts/docker-run.sh --commit $(COMMIT) || ./scripts/docker-run.bat --commit $(COMMIT)

docker-test:
	@echo "Running tests in Docker..."
	docker-compose up gitchangesanalyzer-test

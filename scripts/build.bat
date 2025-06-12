@echo off
setlocal

REM Build script for GitChangesAnalyzer
echo Building GitChangesAnalyzer...

REM Restore dependencies
echo Restoring dependencies...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo Failed to restore dependencies
    exit /b 1
)

REM Build the project
echo Building project...
dotnet build --configuration Release --no-restore
if %ERRORLEVEL% neq 0 (
    echo Build failed
    exit /b 1
)

REM Run tests
echo Running tests...
dotnet test --configuration Release --no-build --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo Tests failed
    exit /b 1
)

echo Build completed successfully!

REM Optional: Create a publish build
set /p choice="Create publish build? (y/N): "
if /i "%choice%"=="y" (
    echo Publishing...
    dotnet publish --configuration Release --no-build --output ./publish
    echo Published to ./publish/
)

pause

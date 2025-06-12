@echo off
REM Setup script for GitChangesAnalyzer development environment

setlocal

echo Setting up GitChangesAnalyzer development environment...

REM Check prerequisites
echo Checking prerequisites...

REM Check .NET SDK
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ❌ .NET SDK not found. Please install .NET 8.0 SDK or later.
    exit /b 1
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do set dotnet_version=%%i
    echo ✅ .NET SDK found: !dotnet_version!
)

REM Check Git
git --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ❌ Git not found. Please install Git.
    exit /b 1
) else (
    for /f "tokens=*" %%i in ('git --version') do set git_version=%%i
    echo ✅ Git found: !git_version!
)

REM Setup project
echo Setting up project...

REM Restore dependencies
echo Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo ❌ Failed to restore packages
    exit /b 1
)

REM Build project
echo Building project...
dotnet build --configuration Debug
if %ERRORLEVEL% neq 0 (
    echo ❌ Build failed
    exit /b 1
)

REM Run tests
echo Running tests...
dotnet test --configuration Debug --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo ⚠️ Some tests failed, but continuing setup...
)

REM Create output directory if it doesn't exist
if not exist "output" (
    echo Creating output directory...
    mkdir output
)

echo.
echo ✅ Setup complete!
echo.
echo Next steps:
echo 1. Try analyzing a commit: .\scripts\Analyze-Commit.ps1 ^<commit-hash^>
echo 2. Or use directly: dotnet run -- --commit ^<commit-hash^>
echo 3. See README.md for more usage examples
echo.
echo Development commands:
echo - Build: .\scripts\build.bat
echo - Clean: .\scripts\clean.bat
echo - Test: dotnet test

pause

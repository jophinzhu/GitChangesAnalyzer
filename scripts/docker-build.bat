@echo off
REM Docker build script for GitChangesAnalyzer

setlocal enabledelayedexpansion

echo Building GitChangesAnalyzer Docker image...

REM Default values
set "BUILD_TARGET=final"
set "IMAGE_TAG=gitchangesanalyzer:latest"
set "NO_CACHE="

REM Parse command line arguments
:parse_args
if "%~1"=="" goto build
if "%~1"=="--dev" (
    set "BUILD_TARGET=build"
    set "IMAGE_TAG=gitchangesanalyzer:dev"
    shift
    goto parse_args
)
if "%~1"=="--test" (
    set "BUILD_TARGET=test"
    set "IMAGE_TAG=gitchangesanalyzer:test"
    shift
    goto parse_args
)
if "%~1"=="--no-cache" (
    set "NO_CACHE=--no-cache"
    shift
    goto parse_args
)
if "%~1"=="--tag" (
    set "IMAGE_TAG=%~2"
    shift
    shift
    goto parse_args
)
if "%~1"=="-h" goto help
if "%~1"=="--help" goto help

echo Unknown option: %~1
exit /b 1

:help
echo Usage: %0 [OPTIONS]
echo Options:
echo   --dev       Build development image
echo   --test      Build test image
echo   --no-cache  Build without using cache
echo   --tag TAG   Specify custom image tag
echo   -h, --help  Show this help
exit /b 0

:build
echo Building target: %BUILD_TARGET%
echo Image tag: %IMAGE_TAG%

REM Build the Docker image
docker build --target "%BUILD_TARGET%" --tag "%IMAGE_TAG%" %NO_CACHE% .

if %ERRORLEVEL% equ 0 (
    echo ✅ Docker image built successfully: %IMAGE_TAG%
    
    REM Show image size
    docker images "%IMAGE_TAG%" --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"
    
    echo.
    echo Usage examples:
    echo   docker run --rm %IMAGE_TAG% --help
    echo   docker run --rm -v "%cd%:/workspace" -v "%cd%/output:/app/output" %IMAGE_TAG% --commit abc123
    echo.
    echo Or use docker-compose:
    echo   docker-compose up gitchangesanalyzer
) else (
    echo ❌ Docker build failed
    exit /b 1
)

pause

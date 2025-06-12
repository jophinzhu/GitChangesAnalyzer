@echo off
REM Docker run script for GitChangesAnalyzer

setlocal enabledelayedexpansion

REM Default values
set "REPO_PATH=%cd%"
set "OUTPUT_PATH=%cd%\output"
set "IMAGE_TAG=gitchangesanalyzer:latest"
set "COMMIT_HASH="
set "EXTRA_ARGS="

:parse_args
if "%~1"=="" goto run
if "%~1"=="--repo" (
    set "REPO_PATH=%~2"
    shift
    shift
    goto parse_args
)
if "%~1"=="--output" (
    set "OUTPUT_PATH=%~2"
    shift
    shift
    goto parse_args
)
if "%~1"=="--image" (
    set "IMAGE_TAG=%~2"
    shift
    shift
    goto parse_args
)
if "%~1"=="--commit" (
    set "COMMIT_HASH=%~2"
    shift
    shift
    goto parse_args
)
if "%~1"=="-h" goto help
if "%~1"=="--help" goto help

REM Collect remaining arguments
set "EXTRA_ARGS=!EXTRA_ARGS! %~1"
shift
goto parse_args

:help
echo Usage: %0 [OPTIONS] [ANALYZER_ARGS]
echo Docker run wrapper for GitChangesAnalyzer
echo.
echo Options:
echo   --repo PATH     Path to git repository (default: current directory)
echo   --output PATH   Path for output files (default: .\output)
echo   --image TAG     Docker image tag (default: gitchangesanalyzer:latest)
echo   --commit HASH   Git commit hash to analyze
echo   -h, --help      Show this help
echo.
echo Any additional arguments are passed to GitChangesAnalyzer
echo.
echo Examples:
echo   %0 --commit abc123 --verbose
echo   %0 --repo C:\path\to\repo --commit abc123..def456
echo   %0 --help  # Show GitChangesAnalyzer help
exit /b 0

:run
REM Validate inputs
if not exist "%REPO_PATH%" (
    echo ❌ Repository path does not exist: %REPO_PATH%
    exit /b 1
)

if not exist "%REPO_PATH%\.git" (
    echo ❌ Not a git repository: %REPO_PATH%
    exit /b 1
)

REM Create output directory if it doesn't exist
if not exist "%OUTPUT_PATH%" mkdir "%OUTPUT_PATH%"

REM Build analyzer arguments
set "ANALYZER_ARGS="
if not "%COMMIT_HASH%"=="" (
    set "ANALYZER_ARGS=--commit %COMMIT_HASH%"
)
set "ANALYZER_ARGS=%ANALYZER_ARGS% %EXTRA_ARGS%"

REM Convert Windows paths to Docker-compatible paths
set "DOCKER_REPO_PATH=%REPO_PATH:\=/%"
set "DOCKER_OUTPUT_PATH=%OUTPUT_PATH:\=/%"
REM Handle drive letters (C: -> /c)
set "DOCKER_REPO_PATH=%DOCKER_REPO_PATH:C:=/c%"
set "DOCKER_OUTPUT_PATH=%DOCKER_OUTPUT_PATH:C:=/c%"

echo Running GitChangesAnalyzer in Docker...
echo Repository: %REPO_PATH%
echo Output: %OUTPUT_PATH%
echo Image: %IMAGE_TAG%
echo Arguments: %ANALYZER_ARGS%
echo.

REM Run the Docker container
docker run --rm ^
    -v "%DOCKER_REPO_PATH%:/workspace:ro" ^
    -v "%DOCKER_OUTPUT_PATH%:/app/output" ^
    "%IMAGE_TAG%" ^
    %ANALYZER_ARGS%

if %ERRORLEVEL% equ 0 (
    echo.
    echo ✅ Analysis complete! Check output directory: %OUTPUT_PATH%
    dir "%OUTPUT_PATH%"
) else (
    echo ❌ Analysis failed
    exit /b 1
)

pause

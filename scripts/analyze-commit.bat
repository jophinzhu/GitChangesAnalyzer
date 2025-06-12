@echo off
setlocal

REM Git Changes Analyzer - Easy launcher script
REM Usage: analyze-commit.bat <commit-hash> [output-file]

if "%1"=="" (
    echo Usage: analyze-commit.bat ^<commit-hash^> [output-file]
    echo.
    echo Examples:
    echo   analyze-commit.bat abc123
    echo   analyze-commit.bat abc123 my-analysis.md
    echo   analyze-commit.bat abc123..def456 range-analysis.md
    exit /b 1
)

set COMMIT=%1
set OUTPUT=%2

REM Default output file if not specified
if "%OUTPUT%"=="" (
    set OUTPUT=commit-analysis-%COMMIT:~0,8%.md
)

REM Change to tool directory
cd /d "%~dp0"

echo Analyzing commit: %COMMIT%
echo Output file: %OUTPUT%
echo.

REM Run the analysis
if "%COMMIT:..=%" neq "%COMMIT%" (
    REM Contains ".." so it's a range
    dotnet run -- --commit-range "%COMMIT%" --output "%OUTPUT%" --verbose
) else (
    REM Single commit
    dotnet run -- --commit "%COMMIT%" --output "%OUTPUT%" --verbose
)

if %ERRORLEVEL% equ 0 (
    echo.
    echo Analysis complete! Report saved to: %OUTPUT%
    echo Opening report...
    start "" "%OUTPUT%"
) else (
    echo.
    echo Analysis failed with error code: %ERRORLEVEL%
)

pause

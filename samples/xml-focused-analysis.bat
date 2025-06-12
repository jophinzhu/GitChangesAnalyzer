@echo off
REM XML-focused analysis sample
REM This script analyzes only XML files with XML-specific mode enabled

setlocal

if "%1"=="" (
    echo Usage: xml-focused-analysis.bat ^<commit-hash^>
    echo Example: xml-focused-analysis.bat abc123
    exit /b 1
)

set COMMIT=%1
set OUTPUT=xml-analysis-%COMMIT:~0,8%-%date:~-4,4%%date:~-10,2%%date:~-7,2%.md

echo Analyzing XML files in commit: %COMMIT%
echo Output will be saved to: %OUTPUT%
echo.

REM Run XML-focused analysis
dotnet run -- ^
    --commit "%COMMIT%" ^
    --include "*.xml,*.config,*.xaml,*.xsl,*.xslt" ^
    --xml-mode ^
    --similarity-threshold 0.85 ^
    --output "%OUTPUT%" ^
    --verbose

if %ERRORLEVEL% equ 0 (
    echo.
    echo XML analysis complete! Report saved to: %OUTPUT%
    echo Opening report...
    start "" "%OUTPUT%"
) else (
    echo.
    echo Analysis failed with error code: %ERRORLEVEL%
)

pause

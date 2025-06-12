@echo off
REM Clean build artifacts and reset the project to a clean state

setlocal

echo Cleaning GitChangesAnalyzer project...

REM Remove build artifacts
echo Removing build artifacts...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
if exist publish rmdir /s /q publish
if exist tests\bin rmdir /s /q tests\bin
if exist tests\obj rmdir /s /q tests\obj

REM Remove temporary files
echo Removing temporary files...
for /r . %%i in (*.tmp) do del "%%i" 2>nul
for /r . %%i in (*.temp) do del "%%i" 2>nul
for /r . %%i in (*~) do del "%%i" 2>nul

REM Remove IDE-specific files
echo Removing IDE files...
if exist .vs rmdir /s /q .vs 2>nul
if exist .vscode\settings.json del .vscode\settings.json 2>nul
if exist .idea rmdir /s /q .idea 2>nul

REM Clean NuGet cache (optional)
set /p choice="Clean NuGet package cache? (y/N): "
if /i "%choice%"=="y" (
    echo Cleaning NuGet cache...
    dotnet nuget locals all --clear
)

echo Project cleaned successfully!
echo You can now run 'dotnet restore' and 'dotnet build' to rebuild the project.

pause

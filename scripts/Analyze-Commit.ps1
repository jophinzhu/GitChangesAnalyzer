# Git Changes Analyzer - PowerShell launcher
# More advanced features and error handling

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$Commit,
    
    [Parameter(Position=1)]
    [string]$OutputPath,
    
    [string]$Format = "markdown",
    [string]$Include,
    [string]$Exclude,
    [switch]$XmlMode,
    [double]$SimilarityThreshold = 0.8,
    [switch]$Verbose
)

# Set default output path if not specified
if (-not $OutputPath) {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    if ($Commit -like "*..") {
        $OutputPath = "commit-range-analysis-$timestamp.md"
    } else {
        $commitShort = $Commit.Substring(0, [Math]::Min(8, $Commit.Length))
        $OutputPath = "commit-analysis-$commitShort-$timestamp.md"
    }
}

# Change to tool directory
$toolDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $toolDir

try {
    Write-Host "Git Changes Analyzer" -ForegroundColor Green
    Write-Host "===================" -ForegroundColor Green
    Write-Host "Commit/Range: $Commit"
    Write-Host "Output: $OutputPath"
    Write-Host "Format: $Format"
    
    if ($Include) { Write-Host "Include patterns: $Include" }
    if ($Exclude) { Write-Host "Exclude patterns: $Exclude" }
    if ($XmlMode) { Write-Host "XML mode: Enabled" -ForegroundColor Yellow }
    Write-Host "Similarity threshold: $SimilarityThreshold"
    Write-Host ""

    # Build command arguments
    $args = @()
    
    if ($Commit -like "*..") {
        $args += "--commit-range", $Commit
    } else {
        $args += "--commit", $Commit
    }
    
    $args += "--output", $OutputPath
    $args += "--format", $Format
    
    if ($Include) { $args += "--include", $Include }
    if ($Exclude) { $args += "--exclude", $Exclude }
    if ($XmlMode) { $args += "--xml-mode" }
    if ($Verbose) { $args += "--verbose" }
    
    $args += "--similarity-threshold", $SimilarityThreshold.ToString("F2")

    # Run the analysis
    Write-Host "Starting analysis..." -ForegroundColor Yellow
    $startTime = Get-Date
    
    & dotnet run -- @args
    
    if ($LASTEXITCODE -eq 0) {
        $endTime = Get-Date
        $duration = $endTime - $startTime
        
        Write-Host ""
        Write-Host "Analysis completed successfully!" -ForegroundColor Green
        Write-Host "Duration: $($duration.TotalSeconds.ToString('F1')) seconds"
        Write-Host "Report saved to: $OutputPath"
          # Open the report (always markdown now)
        $openFile = Read-Host "Open the report file? (y/N)"
        if ($openFile -eq "y" -or $openFile -eq "Y") {
            Start-Process $OutputPath
        }
        
        # Show file size
        if (Test-Path $OutputPath) {
            $fileSize = [Math]::Round((Get-Item $OutputPath).Length / 1KB, 1)
            Write-Host "Report size: $fileSize KB"
        }
    } else {
        Write-Host ""
        Write-Host "Analysis failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

# Examples of usage:
<#
# Basic usage
.\Analyze-Commit.ps1 abc123

# XML-focused analysis
.\Analyze-Commit.ps1 abc123 -XmlMode -Include "*.xml,*.config"

# Commit range with JSON output
.\Analyze-Commit.ps1 "abc123..def456" -Format json

# High similarity threshold for strict grouping
.\Analyze-Commit.ps1 abc123 -SimilarityThreshold 0.95 -Verbose

# Exclude generated files
.\Analyze-Commit.ps1 abc123 -Exclude "*.designer.cs,bin/*,obj/*"
#>

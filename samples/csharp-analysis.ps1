# C# Code Analysis Sample
# This script analyzes C# code files with appropriate filters

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$Commit,
    
    [string]$OutputPath,
    [double]$SimilarityThreshold = 0.8
)

# Set default output path if not specified
if (-not $OutputPath) {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $commitShort = $Commit.Substring(0, [Math]::Min(8, $Commit.Length))
    $OutputPath = "csharp-analysis-$commitShort-$timestamp.md"
}

Write-Host "C# Code Analysis" -ForegroundColor Green
Write-Host "===============" -ForegroundColor Green
Write-Host "Commit: $Commit"
Write-Host "Output: $OutputPath"
Write-Host "Similarity Threshold: $SimilarityThreshold"
Write-Host ""

try {
    # Build arguments for C# analysis
    $args = @(
        "--commit", $Commit
        "--include", "*.cs"
        "--exclude", "*.designer.cs,*.g.cs,*.g.i.cs,*AssemblyInfo.cs,bin/*,obj/*"
        "--output", $OutputPath
        "--similarity-threshold", $SimilarityThreshold.ToString("F2")
        "--verbose"
    )
    
    Write-Host "Starting C# code analysis..." -ForegroundColor Yellow
    $startTime = Get-Date
    
    & dotnet run -- @args
    
    if ($LASTEXITCODE -eq 0) {
        $endTime = Get-Date
        $duration = $endTime - $startTime
        
        Write-Host ""
        Write-Host "Analysis completed successfully!" -ForegroundColor Green
        Write-Host "Duration: $($duration.TotalSeconds.ToString('F1')) seconds"
        Write-Host "Report saved to: $OutputPath"
        
        # Show file size
        if (Test-Path $OutputPath) {
            $fileSize = [Math]::Round((Get-Item $OutputPath).Length / 1KB, 1)
            Write-Host "Report size: $fileSize KB"
        }
        
        # Open the report
        $openFile = Read-Host "Open the report file? (y/N)"
        if ($openFile -eq "y" -or $openFile -eq "Y") {
            Start-Process $OutputPath
        }
    } else {
        Write-Host ""
        Write-Host "Analysis failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Usage examples:
<#
# Basic C# analysis
.\csharp-analysis.ps1 abc123

# With custom similarity threshold
.\csharp-analysis.ps1 abc123 -SimilarityThreshold 0.9

# With custom output path
.\csharp-analysis.ps1 abc123 -OutputPath "my-csharp-report.md"
#>

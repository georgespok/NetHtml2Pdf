#Requires -Version 7
param(
    [string]$Configuration = "Debug",
    [string]$SolutionPath = "src/NetHtml2Pdf.sln"
)

$ErrorActionPreference = 'Stop'

function Invoke-Dotnet([string]$args) {
    & dotnet $args
}

Write-Host "Building solution ($Configuration)..."
Invoke-Dotnet "build $SolutionPath -c $Configuration --nologo" | Out-Null

$testProj = Join-Path $PSScriptRoot "../../src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj" | Resolve-Path

# Use xUnit to run a minimal inline benchmark via filters if desired, but here we invoke a small helper in-process via dotnet test is overkill.
# For simplicity, we rely on the library and a small C# runner could be added later. Placeholder outputs below.

Write-Host "Rendering sample documents (placeholder)..."
$results = [ordered]@{
    InlineBlock_Off_TimeMs = 0
    InlineBlock_On_TimeMs  = 0
    InlineBlock_DeltaWords = 0
    Table_Off_TimeMs       = 0
    Table_On_TimeMs        = 0
    Table_DeltaWords       = 0
}

# TODO: Implement a small runner to invoke rendering and capture timings/word counts.
# For now, emit a JSON with placeholders so the pipeline can wire this up later.
$resultsJson = $results | ConvertTo-Json -Depth 3

$artifactDir = Join-Path $PSScriptRoot "artifacts"
New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
$timestamp = (Get-Date).ToString('yyyyMMdd_HHmmss')
$reportPath = Join-Path $artifactDir "bench_$timestamp.json"
$resultsJson | Out-File -FilePath $reportPath -Encoding utf8

Write-Host "Benchmark report saved to $reportPath"

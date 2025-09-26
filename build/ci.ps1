param(
  [string]$Configuration = "Release",
  [switch]$Pack,
  [string]$VersionSuffix = "beta.1"
)

$ErrorActionPreference = 'Stop'

Write-Host "Restoring..."
dotnet restore | Out-Null

Write-Host "Building ($Configuration)..."
dotnet build .\src\NetHtml2Pdf.sln -c $Configuration -v minimal | Out-Null

Write-Host "Running tests..."
dotnet test .\src\NetHtml2Pdf.sln -c $Configuration --no-build --logger "trx;LogFileName=test-results.trx" | Out-Null

if ($Pack) {
  Write-Host "Packing NuGet..."
  if ($VersionSuffix -and $VersionSuffix.Trim()) {
    & $PSScriptRoot\build-nuget.ps1 -Configuration $Configuration -VersionSuffix $VersionSuffix
  } else {
    & $PSScriptRoot\build-nuget.ps1 -Configuration $Configuration
  }
}

Write-Host "CI tasks complete."



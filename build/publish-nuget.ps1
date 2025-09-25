param(
  [Parameter(Mandatory=$true)][string]$ApiKey,
  [string]$Source = "https://api.nuget.org/v3/index.json"
)

$ErrorActionPreference = 'Stop'

$nugetDir = Join-Path $PSScriptRoot "..\nuget"
if (!(Test-Path $nugetDir)) {
  throw "Nuget folder not found: $nugetDir. Run build-nuget.ps1 first."
}

$packages = Get-ChildItem -Path $nugetDir -Filter "*.nupkg" -File
if (!$packages) {
  throw "No .nupkg files found in $nugetDir."
}

foreach ($pkg in $packages) {
  Write-Host "Pushing $($pkg.Name) to $Source..."
  dotnet nuget push $pkg.FullName --api-key $ApiKey --source $Source --skip-duplicate
}

Write-Host "Publish complete."

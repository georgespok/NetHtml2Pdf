param(
  [string]$Configuration = "Release",
  [switch]$Pack,
  [string]$VersionSuffix = "beta.1",
  [switch]$Publish,
  [string]$ApiKey = "",
  [string]$SourceName = "",
  [string]$NuGetConfigPath = ""
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

if ($Publish) {
  Write-Host "Publishing packages..."
  $nugetDir = Join-Path $PSScriptRoot "..\nuget"
  if (!(Test-Path $nugetDir)) {
    throw "Nuget folder not found: $nugetDir. Run with -Pack first."
  }

  $packages = Get-ChildItem -Path $nugetDir -Filter "*.nupkg" -File | Sort-Object Name
  if (!$packages) {
    throw "No .nupkg files found in $nugetDir."
  }

  foreach ($pkg in $packages) {
    $args = @("nuget", "push", $pkg.FullName, "--skip-duplicate")
    if ($ApiKey -and $ApiKey.Trim()) { $args += @("--api-key", $ApiKey) }
    if ($SourceName -and $SourceName.Trim()) { $args += @("--source", $SourceName) }
    if ($NuGetConfigPath -and $NuGetConfigPath.Trim()) { $args += @("--configfile", $NuGetConfigPath) }

    Write-Host "Pushing $($pkg.Name)..."
    dotnet @args
  }
}

Write-Host "CI tasks complete."



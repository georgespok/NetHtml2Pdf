param(
  [string]$Configuration = "Release",
  [string]$VersionSuffix = ""
)

$ErrorActionPreference = 'Stop'

Write-Host "Restoring..."
dotnet restore | Out-Null

Write-Host "Building ($Configuration)..."
dotnet build .\src\NetHtml2Pdf.sln -c $Configuration -v minimal | Out-Null

$outputDir = Join-Path $PSScriptRoot "..\nuget"
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$proj = ".\src\NetHtml2Pdf\NetHtml2Pdf.csproj"
$args = @(
  "pack",
  $proj,
  "-c", $Configuration,
  "-o", $outputDir,
  "/p:ContinuousIntegrationBuild=true"
)
if ($VersionSuffix -and $VersionSuffix.Trim()) {
  $args += @("/p:VersionSuffix=$VersionSuffix")
}

Write-Host "Packing NuGet package..."
dotnet @args | Out-Null

Write-Host "Done. Packages in: $(Resolve-Path $outputDir)"

param(
  [string]$Configuration = "Release",
  [string]$VersionSuffix = "beta.1"
)

$ErrorActionPreference = 'Stop'

Write-Host "Restoring..."
dotnet restore .\src\NetHtml2Pdf.sln
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }

Write-Host "Building ($Configuration)..."
dotnet build .\src\NetHtml2Pdf.sln -c $Configuration -v minimal
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }

$outputDir = Join-Path $PSScriptRoot "..\nuget"
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$projectsToPack = @(  
  ".\src\NetHtml2Pdf\NetHtml2Pdf.csproj"
)

Write-Host "Packing NuGet packages..."
foreach ($proj in $projectsToPack) {
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
  dotnet @args
  if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed for $proj with exit code $LASTEXITCODE" }
}

Write-Host "Done. Packages in: $(Resolve-Path $outputDir)"

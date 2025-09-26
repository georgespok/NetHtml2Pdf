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

$projectsToPack = @(
  ".\src\NetHtml2Pdf.Core\NetHtml2Pdf.Core.csproj",
  ".\src\NetHtml2Pdf.Parser\NetHtml2Pdf.Parser.csproj",
  ".\src\NetHtml2Pdf.Renderer\NetHtml2Pdf.Renderer.csproj",
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
  dotnet @args | Out-Null
}

Write-Host "Done. Packages in: $(Resolve-Path $outputDir)"

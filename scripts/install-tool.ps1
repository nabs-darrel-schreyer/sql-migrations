#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory=$false)]
    [string]$TestSearchPath
)

# Get the root directory (assumes script is in scripts folder)
$rootDir = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $rootDir "src\SqlMigrations.MigrationCli\SqlMigrations.MigrationCli.csproj"
$toolCommandName = "nabs-migrations"

Write-Host "Packing SqlMigrations.MigrationCli..." -ForegroundColor Cyan

# Pack the project
dotnet pack $projectPath --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to pack the project" -ForegroundColor Red
    exit 1
}

Write-Host "Pack completed successfully" -ForegroundColor Green

# Get the nupkg file path
$nupkgDir = Join-Path $rootDir "src\SqlMigrations.MigrationCli\nupkg"
$nupkgFile = Get-ChildItem -Path $nupkgDir -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $nupkgFile) {
    Write-Host "No .nupkg file found in $nupkgDir" -ForegroundColor Red
    exit 1
}

Write-Host "Found package: $($nupkgFile.FullName)" -ForegroundColor Cyan

# Check if tool is already installed
$installedTools = dotnet tool list --global
if ($installedTools -match $toolCommandName) {
    Write-Host "Uninstalling existing tool..." -ForegroundColor Yellow
    dotnet tool uninstall SqlMigrations.MigrationCli --global
}

# Install the tool globally
Write-Host "Installing tool globally..." -ForegroundColor Cyan
dotnet tool install --global --add-source $nupkgDir SqlMigrations.MigrationCli

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to install the tool" -ForegroundColor Red
    exit 1
}

Write-Host "Tool installed successfully! You can now use '$toolCommandName' command." -ForegroundColor Green

# Test the tool if TestSearchPath parameter is provided
if ($TestSearchPath) {
    Write-Host "`nTesting tool with search path: $TestSearchPath" -ForegroundColor Cyan
    & $toolCommandName $TestSearchPath
} else {
    Write-Host "`nUsage examples:" -ForegroundColor Yellow
    Write-Host "  $toolCommandName" -ForegroundColor White
    Write-Host "  $toolCommandName C:\Path\To\Solution" -ForegroundColor White
}

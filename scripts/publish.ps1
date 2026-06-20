# Publish AVA Security desktop app (Windows x64)
param(
    [string]$Configuration = "Release",
    [string]$OutputDir = ".\dist\AVASecurity"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Join-Path $root "..")

Write-Host "Building AVA Security..." -ForegroundColor Cyan
dotnet restore avasec.sln
dotnet build avasec.sln -c $Configuration
dotnet test avasec.sln -c $Configuration --no-build

if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}

dotnet publish avasec.ui/avasec.ui.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained false `
    -o $OutputDir `
    /p:PublishSingleFile=false

Write-Host "Published to $OutputDir" -ForegroundColor Green
Write-Host "Run: .\dist\AVASecurity\AVASecurity.exe" -ForegroundColor Yellow

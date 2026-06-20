# Quick start — AVA Security desktop + optional portal
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Join-Path $root "..")

Write-Host "Starting AVA Security desktop..." -ForegroundColor Cyan
dotnet run --project avasec.ui

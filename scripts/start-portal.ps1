# Start AVA Security web portal (avasec.server)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$serverDir = Join-Path $root "..\avasec.server"
Set-Location $serverDir

if (-not (Test-Path ".env")) {
    Copy-Item ".env.example" ".env"
    Write-Host "Created .env from .env.example" -ForegroundColor Yellow
}

if (-not (Test-Path "node_modules")) {
    Write-Host "Installing npm packages..." -ForegroundColor Cyan
    npm install
}

Write-Host ""
Write-Host "Portal URLs:" -ForegroundColor Green
Write-Host "  Login:  http://localhost:3001/index.html"
Write-Host "  Store:  http://localhost:3001/store.html"
Write-Host "  Admin:  http://localhost:3001/admin.html"
Write-Host "  Health: http://localhost:3001/api/health"
Write-Host ""
Write-Host "Press Ctrl+C to stop." -ForegroundColor Gray
Write-Host ""

node server.js

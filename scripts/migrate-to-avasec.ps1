# Migrate avasec/SysAnti -> avasec / AVA Security
$ErrorActionPreference = "Stop"
$root = "E:\DEV28\Antigravity\avasec"
Set-Location $root

Write-Host "=== AVA Security migration (avasec) ===" -ForegroundColor Cyan

$folderRenames = @(
    @("plugins\avasec.plugins.systemsweeper", "plugins\avasec.plugins.systemsweeper"),
    @("plugins\avasec.plugins.registrydoctor", "plugins\avasec.plugins.registrydoctor"),
    @("plugins\avasec.plugins.privacyshredder", "plugins\avasec.plugins.privacyshredder"),
    @("plugins\avasec.plugins.networkfortress", "plugins\avasec.plugins.networkfortress"),
    @("plugins\avasec.plugins.gamebooster", "plugins\avasec.plugins.gamebooster"),
    @("avasec.chat.core", "avasec.chat.core"),
    @("avasec.plugin.core", "avasec.plugin.core"),
    @("avasec.authentication", "avasec.authentication"),
    @("avasec.optimization", "avasec.optimization"),
    @("avasec.antivirus", "avasec.antivirus"),
    @("avasec.database", "avasec.database"),
    @("avasec.tests", "avasec.tests"),
    @("avasec.mini", "avasec.mini"),
    @("avasec.server", "avasec.server"),
    @("avasec.core", "avasec.core"),
    @("avasec.ui", "avasec.ui")
)

foreach ($pair in $folderRenames) {
    $from = Join-Path $root $pair[0]
    $to = Join-Path $root $pair[1]
    if (Test-Path $from) {
        $parent = Split-Path $to -Parent
        if ($parent -and -not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
        if (-not (Test-Path $to)) {
            Move-Item -LiteralPath $from -Destination $to
            Write-Host "  folder: $($pair[0]) -> $($pair[1])"
        }
    }
}

if (Test-Path (Join-Path $root "Plugins")) {
    $remaining = Get-ChildItem (Join-Path $root "Plugins") -Force -ErrorAction SilentlyContinue
    if (-not $remaining) { Remove-Item (Join-Path $root "Plugins") -Force -ErrorAction SilentlyContinue }
}

Get-ChildItem -Path $root -Directory -Recurse | Where-Object { $_.FullName -notmatch '\\(bin|obj|node_modules)\\' } | ForEach-Object {
    Get-ChildItem $_.FullName -Filter "SysAnti*.csproj" -File -ErrorAction SilentlyContinue | ForEach-Object {
        $newName = ($_.Name -creplace 'SysAnti', 'avasec').ToLower()
        Move-Item $_.FullName (Join-Path $_.DirectoryName $newName) -Force
    }
}

if (Test-Path "$root\avasec.sln") { Move-Item "$root\avasec.sln" "$root\avasec.sln" -Force }
if (Test-Path "$root\install\avasec-setup.iss") { Move-Item "$root\install\avasec-setup.iss" "$root\install\avasec-setup.iss" -Force }

# Rename key source files
$ctxOld = Get-ChildItem -Path $root -Recurse -Filter "AVASecContext.cs" -File | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
foreach ($f in $ctxOld) {
    Move-Item $f.FullName (Join-Path $f.DirectoryName "AVASecContext.cs") -Force
}

$skip = @('bin','obj','node_modules','.git')
$ext = @('*.cs','*.csproj','*.sln','*.xaml','*.json','*.md','*.html','*.js','*.ps1','*.iss','*.bat','*.sql')
$files = Get-ChildItem $root -Recurse -File -Include $ext | Where-Object {
    $p = $_.FullName; -not ($skip | ForEach-Object { $p -match "\\$_\\" } | Where-Object { $_ })
}

$pathMap = @{
    'avasec.plugins.systemsweeper' = 'avasec.plugins.systemsweeper'
    'avasec.plugins.registrydoctor' = 'avasec.plugins.registrydoctor'
    'avasec.plugins.privacyshredder' = 'avasec.plugins.privacyshredder'
    'avasec.plugins.networkfortress' = 'avasec.plugins.networkfortress'
    'avasec.plugins.gamebooster' = 'avasec.plugins.gamebooster'
    'avasec.plugins' = 'avasec.plugins'
    'avasec.chat.core' = 'avasec.chat.core'
    'avasec.plugin.core' = 'avasec.plugin.core'
    'avasec.authentication' = 'avasec.authentication'
    'avasec.optimization' = 'avasec.optimization'
    'avasec.antivirus' = 'avasec.antivirus'
    'avasec.database' = 'avasec.database'
    'avasec.tests' = 'avasec.tests'
    'avasec.mini' = 'avasec.mini'
    'avasec.server' = 'avasec.server'
    'avasec.core' = 'avasec.core'
    'avasec.ui' = 'avasec.ui'
}

foreach ($file in $files) {
    $c = [IO.File]::ReadAllText($file.FullName)
    $o = $c

    if ($file.Extension -in '.csproj','.sln','.ps1','.iss','.yml','.json','.bat') {
        foreach ($k in $pathMap.Keys) { $c = $c.Replace($k, $pathMap[$k]) }
    }

    $c = $c.Replace('namespace AVASec', 'namespace AVASec')
    $c = $c.Replace('using AVASec', 'using AVASec')
    $c = $c.Replace('AVASecContext', 'AVASecContext')
    $c = $c.Replace('AVASecMini', 'AVASecMini')
    $c = $c.Replace('AVASecurity', 'AVASecurity')
    $c = $c.Replace('AVASecurity', 'AVASecurity')
    $c = $c.Replace('AVA Security Team', 'AVA Security Team')
    $c = $c.Replace('avasec-server', 'avasec-server')
    $c = $c.Replace('AVA Security Server', 'AVA Security Server')
    $c = $c.Replace('avasec_db', 'avasec_db')
    $c = $c.Replace('avasec.db', 'avasec.db')
    $c = $c.Replace('avasec', 'avasec')
    $c = $c.Replace('AVA Security', 'AVA Security')
    $c = $c.Replace('AVA Mind', 'AVA Mind')
    $c = $c.Replace('AVA-', 'AVA-')
    $c = $c.Replace('avasec-setup', 'avasec-setup')
    $c = $c.Replace('AVASecurity', 'AVASecurity')
    $c = $c.Replace('AVA Shield', 'AVA Shield')
    $c = $c.Replace('avasec.app', 'avasec.app')
    $c = $c.Replace('support@avasec', 'support@avasec')
    $c = $c.Replace('admin@avasec.app', 'admin@avasec.app')
    $c = $c.Replace('AVA Security', 'AVA Security')
    $c = $c.Replace('AVASecurity', 'AVA Security')

    if ($file.Extension -in '.xaml','.html','.md','.js') {
        $c = $c.Replace('>Vigil<', '>AVA Security<')
        $c = $c.Replace('Vigil', 'AVA Security')
    }

    if ($c -ne $o) { [IO.File]::WriteAllText($file.FullName, $c) }
}

Write-Host "Done. Building..." -ForegroundColor Green

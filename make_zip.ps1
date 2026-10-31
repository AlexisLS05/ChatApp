# Crée LAUNAY-SCHAPMAN_AUGAY_Project.zip : solution (sans bin/, obj/, .git/, .vs/) + docs/report.pdf
# Usage : powershell -ExecutionPolicy Bypass -File .\make_zip.ps1
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$zipName = "LAUNAY-SCHAPMAN_AUGAY_Project.zip"
$zipPath = Join-Path $root $zipName
$report = Join-Path $root "docs\report.pdf"

if (-not (Test-Path $report)) {
    Write-Host "docs\report.pdf introuvable : imprime docs\report.html en PDF d'abord." -ForegroundColor Red
    exit 1
}

# Dossier temporaire de préparation
$staging = Join-Path ([System.IO.Path]::GetTempPath()) ("ChatApp_zip_" + [guid]::NewGuid())
$dest = Join-Path $staging "ChatApp"
New-Item -ItemType Directory -Force $dest | Out-Null

$excluded = @("bin", "obj", ".git", ".vs", ".vscode")

# Copie récursive en sautant les dossiers exclus
function Copy-Filtered($src, $dst) {
    New-Item -ItemType Directory -Force $dst | Out-Null
    foreach ($item in Get-ChildItem -LiteralPath $src -Force) {
        if ($item.PSIsContainer) {
            if ($excluded -notcontains $item.Name) {
                Copy-Filtered $item.FullName (Join-Path $dst $item.Name)
            }
        } else {
            Copy-Item -LiteralPath $item.FullName -Destination $dst
        }
    }
}

Copy-Item -LiteralPath (Join-Path $root "ChatApp.sln") -Destination $dest
Copy-Filtered (Join-Path $root "ChatServer") (Join-Path $dest "ChatServer")
Copy-Filtered (Join-Path $root "ChatClient") (Join-Path $dest "ChatClient")
New-Item -ItemType Directory -Force (Join-Path $dest "docs") | Out-Null
Copy-Item -LiteralPath $report -Destination (Join-Path $dest "docs")

# Le journal d'exécution n'a rien à faire dans le rendu
Get-ChildItem -LiteralPath $dest -Recurse -Filter "server.log" | Remove-Item -Force

if (Test-Path $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
Compress-Archive -Path $dest -DestinationPath $zipPath
Remove-Item -LiteralPath $staging -Recurse -Force

Write-Host "Archive créée : $zipPath"
Get-ChildItem -LiteralPath $root -Filter $zipName | Select-Object Name, Length

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $projectRoot
try {
    $presenceBuildRoot = Join-Path $env:LOCALAPPDATA 'PresenceBuild'
    dotnet publish src/Presence.App/Presence.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false --artifacts-path (Join-Path $presenceBuildRoot 'artifacts') -o (Join-Path $presenceBuildRoot 'portable')
    if ($LASTEXITCODE -ne 0) { throw 'Production publish failed.' }
    New-Item -ItemType Directory -Force -Path artifacts/portable | Out-Null
    Copy-Item -Path (Join-Path $presenceBuildRoot 'portable\*') -Destination artifacts/portable
    Copy-Item -LiteralPath README.md,THIRD_PARTY_NOTICES.md -Destination artifacts/portable
    Write-Host 'Portable files are in artifacts/portable. Extract together; run Presence.exe.'
} finally {
    Pop-Location
}

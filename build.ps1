$ErrorActionPreference = 'Stop'
$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$outputDir = Join-Path $projectDir 'output'
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$windowsDir = Split-Path -Parent ([Environment]::SystemDirectory)
& "$windowsDir\Microsoft.NET\Framework64\v4.0.30319\csc.exe" `
    /nologo /target:winexe /optimize+ `
    /win32icon:"$projectDir\assets\app-icon.ico" `
    /out:"$outputDir\ChatGPTProxyLauncher.exe" `
    /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll `
    "$projectDir\src\Program.cs"

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Built: $outputDir\ChatGPTProxyLauncher.exe"

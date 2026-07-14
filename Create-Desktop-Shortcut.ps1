$ErrorActionPreference = 'Stop'
$launcher = Join-Path $PSScriptRoot 'ChatGPTProxyLauncher.exe'
if (-not (Test-Path -LiteralPath $launcher)) {
    throw 'ChatGPTProxyLauncher.exe must be in the same folder as this script.'
}

$desktop = [Environment]::GetFolderPath('Desktop')
$shortcutPath = Join-Path $desktop 'ChatGPT Proxy Launcher.lnk'
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $launcher
$shortcut.WorkingDirectory = $PSScriptRoot
$shortcut.IconLocation = "$launcher,0"
$shortcut.Description = 'Start ChatGPT Desktop with a local HTTP proxy'
$shortcut.Save()

Write-Host "Desktop shortcut created: $shortcutPath"

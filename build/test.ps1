$BuildFile = (Split-Path $MyInvocation.MyCommand.Path) + "\build.ps1 -target WatchFiles"
Write-Host "Running >>> " $BuildFile
Invoke-Expression $BuildFile
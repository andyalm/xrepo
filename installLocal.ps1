#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'
dotnet build

$ToolName = 'xrepo'
try {
    dotnet tool uninstall -g $ToolName
}
catch {}

$PathToTool = Resolve-Path "src/CommandLine/bin/Debug"
$Package = Get-ChildItem $PathToTool |
    Where-Object Name -Match 'nupkg' |
    Select-Object -ExpandProperty 'Name'
$Version = $Package.Replace("$ToolName.", '').Replace('.nupkg', '')
dotnet tool install -g --version $Version --add-source $PathToTool $ToolName

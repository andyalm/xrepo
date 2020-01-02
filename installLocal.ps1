$ErrorActionPreference = 'Stop'

$DotPatchVersion='.0-dev'
dotnet build /p:DotPatchVersion=$DotPatchVersion
$Existing=$(dotnet tool list -g | Select-String xrepo)
if($Existing) {
    dotnet tool uninstall -g xrepo
}
dotnet tool install -g --version 2.0$DotPatchVersion --add-source $(Resolve-Path src/CommandLine/bin/Debug) xrepo
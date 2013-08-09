$ThisScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ModulePath = Join-Path $ThisScriptPath XRepoTabExpansion.psm1
Import-Module $ModulePath
Install-XRepoTabExpansion
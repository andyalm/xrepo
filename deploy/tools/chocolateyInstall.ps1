$package_path = split-path -parent (split-path -parent $MyInvocation.MyCommand.Definition)
Write-Host "Current package path: $package_path"

$global_msbuild_hook_script = "$package_path\tools\installGlobalMSBuildHook.ps1"
Start-ChocolateyProcessAsAdmin "& `'$global_msbuild_hook_script`' $package_path"
$package_path = split-path -parent (split-path -parent $MyInvocation.MyCommand.Definition)
Write-Host "Current package path: $package_path"

$global_msbuild_hook_script = "$package_path\tools\installGlobalMSBuildHook.ps1"
Start-ChocolateyProcessAsAdmin "& `'$global_msbuild_hook_script`' $package_path"

#install powershell tab expansion
if(-not (Test-Path $profile)) {
	Write-Host "No powershell profile found. Creating one at '$profile'"
	New-Item $profile -ItemType File -Force
}

Add-Content $profile "`n& '$package_path\powershell\XRepoTabExpansionBootstrapper.ps1'"
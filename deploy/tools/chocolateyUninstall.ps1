$package_path = split-path -parent (split-path -parent $MyInvocation.MyCommand.Definition)
Write-Host "Current package path: $package_path"

$global_msbuild_hook_script = "$package_path\tools\uninstallGlobalMSBuildHook.ps1"
Start-ChocolateyProcessAsAdmin "& `'$global_msbuild_hook_script`' $package_path"

#uninstall powershell tab expansion
if(Test-Path $profile) {
	$profile_content = @(Get-Content $profile | Where { -not ($_ -match "XRepoTabExpansionBootstrapper") })
	$profile_content | Set-Content $profile
}

#uninstall bash tab expansion
$bash_profile = "$env:UserProfile\.bashrc"
if(Test-Path $bash_profile) {
	$bash_profile_content = @(Get-Content $bash_profile | Where { -not ($_ -match "xrepo.bash") } | Where { -not ($_ -match "alias xrepo=") })
	$bash_profile_content | Set-Content $bash_profile
}
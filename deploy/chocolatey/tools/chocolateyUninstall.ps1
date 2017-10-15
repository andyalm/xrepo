$package_path = split-path -parent -resolve (split-path -parent $MyInvocation.MyCommand.Definition)
Write-Host "Current package path: $package_path"

$toolsPath = "$package_path\tools"

pushd "$package_path\tools"
try {
    dotnet XRepo.Installer.dll uninstall "$package_path\build"
}
finally
{
    popd
}

Uninstall-BinFile -Name xrepo

#uninstall powershell tab expansion
#DISABLED UNTIL IT CAN BE STABLE
#if(Test-Path $profile) {
#	$profile_content = @(Get-Content $profile | Where { -not ($_ -match "XRepoTabExpansionBootstrapper") })
#	$profile_content | Set-Content $profile
#}

#uninstall bash tab expansion
$bash_profile = "$env:UserProfile\.bashrc"
if(Test-Path $bash_profile) {
	$bash_profile_content = @(Get-Content $bash_profile | Where { -not ($_ -match "xrepo_completion.bash") } | Where { -not ($_ -match "alias xrepo=") })
	$bash_profile_content | Set-Content $bash_profile
}
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

#install bash tab expansion
$bash_profile = "$env:UserProfile\.bashrc"
if(-not (Test-Path $bash_profile)) {
	Write-Host "No bash profile found. Creating one at '$bash_profile'"
	New-Item $bash_profile -ItemType File -Force
}
$xrepo_bash_path = "$package_path\bash\xrepo_completion.bash" -replace '([a-z]):\\', '/${1}/' -replace '\\', '/'
$xrepo_exe_path = "$package_path\tools\xrepo.exe" -replace '([a-z]):\\', '/${1}/' -replace '\\', '/'
Add-Content $bash_profile "`nalias xrepo='$xrepo_exe_path'"
Add-Content $bash_profile "`n. $xrepo_bash_path"
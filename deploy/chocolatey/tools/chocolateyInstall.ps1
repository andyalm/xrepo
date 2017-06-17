$package_path = split-path -parent -resolve (split-path -parent $MyInvocation.MyCommand.Definition)
Write-Host "Current package path: $package_path"

$toolsPath = "$package_path\tools"

pushd "$package_path\tools"
try {
    dotnet XRepo.Installer.dll install "$package_path\build"
}
finally
{
    popd
}


$profile = Join-Path $(Split-Path -Parent $profile) profile.ps1
$dotNetPath = Get-Command dotnet | Select-Object -ExpandProperty Definition
Write-Host "dotnet cli path is $dotNetPath"
Install-BinFile -Name xrepo -Path $dotNetPath -Command "$package_path\app\xrepo.dll"

#install powershell tab expansion
#DISABLED UNTIL IT CAN BE STABLE
#if(-not (Test-Path $profile)) {
#	Write-Host "No powershell profile found. Creating one at '$profile'"
#	New-Item $profile -ItemType File -Force
#}
#Add-Content $profile "`n& '$package_path\powershell\XRepoTabExpansionBootstrapper.ps1'"

#install bash tab expansion
$bash_profile = "$env:UserProfile\.bashrc"
if(-not (Test-Path $bash_profile)) {
	Write-Host "No bash profile found. Creating one at '$bash_profile'"
	New-Item $bash_profile -ItemType File -Force
}
$xrepo_bash_path = "$package_path\bash\xrepo_completion.bash" -replace '([a-z]):\\', '/${1}/' -replace '\\', '/'
$xrepo_exe_path = "$($env:ChocolateyInstall)\bin\xrepo.exe" -replace '([a-z]):\\', '/${1}/' -replace '\\', '/'
Add-Content $bash_profile "alias xrepo='$xrepo_exe_path'"
Add-Content $bash_profile ". $xrepo_bash_path"
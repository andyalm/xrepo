param($xrepo_root)

Import-Module "$xrepo_root\tools\GlobalMSBuildHook.psm1"

$global_msbuild_hook_paths = Get-GlobalMSBuildHookFiles

foreach($global_msbuild_hook_path in $global_msbuild_hook_paths)
{
    $filename = Split-Path -Leaf $global_msbuild_hook_path
	if($filename -ieq "Custom.After.Microsoft.Common.Targets") {
		Remove-XRepoImport -Path $global_msbuild_hook_path
	}
	else {
		Remove-Item $global_msbuild_hook_path
	}
}
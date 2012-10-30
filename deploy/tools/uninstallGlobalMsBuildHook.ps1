param($xrepo_root)

Import-Module "$xrepo_root\tools\GlobalMSBuildHook.psm1"

$global_msbuild_hook_paths = Get-GlobalMSBuildHookFiles

foreach($global_msbuild_hook_path in $global_msbuild_hook_paths)
{
    Remove-XRepoImport -Path $global_msbuild_hook_path
}
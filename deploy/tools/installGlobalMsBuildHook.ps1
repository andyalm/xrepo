param($xrepo_root)

Import-Module "$xrepo_root\tools\GlobalMSBuildHook.psm1"

try
{
    $global_msbuild_hook_paths = Get-GlobalMSBuildHookFiles

    foreach($global_msbuild_hook_path in $global_msbuild_hook_paths)
    {
        if(-not (Test-Path $global_msbuild_hook_path))
        {
            Copy-Item "$xrepo_root\build\Custom.After.Microsoft.Common.Targets" $global_msbuild_hook_path
        }

        Remove-XRepoImport -Path $global_msbuild_hook_path
        Add-XRepoImport -Path $global_msbuild_hook_path -ImportPath "$xrepo_root\build\XRepo.Build.targets"
    }
}
catch
{
    Write-Error $_
}

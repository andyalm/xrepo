param($xrepo_root)

Import-Module "$xrepo_root\tools\GlobalMSBuildHook.psm1"

try
{
    $global_msbuild_hook_paths = Get-GlobalMSBuildHookFiles

    foreach($global_msbuild_hook_path in $global_msbuild_hook_paths)
    {
        $filename = Split-Path -Leaf $global_msbuild_hook_path
		Write-Host "Processing global hook $filename - $global_msbuild_hook_path"
		if(-not (Test-Path $global_msbuild_hook_path))
        {
            Copy-Item "$xrepo_root\build\$filename" $global_msbuild_hook_path
        }
		Write-Host "Removing XRepoImport $global_msbuild_hook_path"
        Remove-XRepoImport -Path $global_msbuild_hook_path
		$targetsFilename = "XRepo.Build.targets"
		if($filename -ieq "Custom.After.Microsoft.Common.Targets") {
			$targetsFilename = "XRepo.Build.Legacy.targets"
		}
		Write-Host "Adding XRepoImport $xrepo_root\build\$targetsFilename"
        Add-XRepoImport -Path $global_msbuild_hook_path -ImportPath "$xrepo_root\build\$targetsFilename"
    }
}
catch
{
    Write-Error $_
}

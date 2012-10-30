$MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003"
$NameTable = New-Object 'System.Xml.NameTable'
$NamespaceManager = New-Object 'System.Xml.XmlNamespaceManager' $NameTable
$NamespaceManager.AddNamespace("msb", $MSBuildNamespace)


function Add-XRepoImport($Path, $ImportPath)
{
    $project = Get-Project $Path
    Write-Host "Adding import of '$ImportPath' to global MSBuild hook..."
    $import = $project.CreateElement("Import", $MSBuildNamespace)
    $import.SetAttribute("Project", $ImportPath)
    $import.SetAttribute("Condition", "Exists('$ImportPath') and `$(DisableGlobalXRepo)!='true'")
    $project.DocumentElement.AppendChild($import) | Out-Null
    $project.Save($Path)
}
Export-ModuleMember Add-XRepoImport

function Remove-XRepoImport($Path)
{
    $project = Get-Project $Path
    try
    {
        foreach($import in $project.SelectNodes("//msb:Import", $NamespaceManager))
        {
            if($import.GetAttribute("Project").ToLowerInvariant().Contains("xrepo.build.targets"))
            {
                Write-Host $("Removing import of '" + $import.GetAttribute("Project") + "' from global MSBuild hook...")
                $import.ParentNode.RemoveChild($import) | Out-Null
            }
        }
        $project.Save($Path)
    }
    catch {
        Write-Error $_
    }
    
}
Export-ModuleMember Remove-XRepoImport

function Get-GlobalMSBuildHookFiles
{
    @(
        "$env:ProgramFiles\MSBuild\v4.0\Custom.After.Microsoft.Common.Targets"
    )
}
Export-ModuleMember Get-GlobalMSBuildHookFiles

function Get-Project($Path)
{
    try
    {
        $project = New-Object 'System.Xml.XmlDocument' $NameTable
        $project.Load($Path)
    }
    catch {
        Write-Error $_
    }
    return $project
}
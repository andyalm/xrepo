$ThisScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition

$LocalDevAssemblyLocation = Join-Path $ThisScriptPath "src\Core\bin\Debug\XRepo.Core.dll"
$DeployedAssemblyLocation = Join-Path $ThisScriptPath "..\tools\XRepo.Core.dll"

if(Test-Path $LocalDevAssemblyLocation) {
	Add-Type -Path $LocalDevAssemblyLocation
}
else {
	Add-Type -Path $DeployedAssemblyLocation
}

function Get-AssembliesAndRepos {
	$xrepo = [XRepo.Core.XRepoEnvironment]::ForCurrentUser()
	$repos = @($xrepo.RepoRegistry.GetRepos() | % { $_.Name })
	$assemblies = @($xrepo.AssemblyRegistry.GetAssemblies() | % { $_.Name })

	return $repos + $assemblies
}

function Get-XRepoCommands {
	@('assemblies', 'config', 'pin', 'unpin', 'pins', 'repos')
}

function Write-Expansions($lastWord) {
	$input | where { $_.StartsWith($lastWord) } | Write-Output
}

function XRepoTabExpansion($line, $lastWord) {
	$parts = $line.Split(' ')
	if($parts[0] -ne 'xrepo') {
		return $null
	}
	$xrepo = [XRepo.Core.XRepoEnvironment]::ForCurrentUser()
	if($parts.Length -eq 2) {
		Write-Expansions $(Get-XRepoCommands) $lastWord
	}
	switch($parts[1]) {
		'pin' {
			Get-AssembliesAndRepos | Write-Expansions $lastWord
		}
		'unpin' {
			(@('all') + @(Get-AssembliesAndRepos)) | Write-Expansions $lastWord
		}
		'repo' {
			@('register', 'unregister') | Write-Expansions $lastWord
		}
		'config' {
			@('copy_pins', 'pin_warnings', 'auto_build_pins') | Write-Expansions $lastWord
		}
		default {
			return $null
		}
	}
}

function TabExpansion_XRepoDecorator($line, $lastWord) {
	$result = XRepoTabExpansion $line $lastWord
	if($result -eq $null) {
		return TabExpansion_Inner $line $lastWord
	}
	else {
		return $result
	}
}


cp function:TabExpansion function:TabExpansion_Inner
cp function:TabExpansion_XRepoDecorator function:TabExpansion
#cp function:XRepoTabExpansion function:TabExpansion
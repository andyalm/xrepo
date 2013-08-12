$ThisScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition

function Test-FileLock($Path) {
	$oFile = New-Object System.IO.FileInfo $Path
	
	if ((Test-Path -Path $Path) -eq $false)	{
		return $false
	}
	  
	try
	{
		$oStream = $oFile.Open([System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
		$oStream.Close()
		
		return $false
	}
	catch
	{
		# file is locked by a process.
		return $true
	}
}

function Load-XRepoAssembly($Path) {
	#file locking sucks. Shadow copy the dll's to a temp dir before they are loaded
	
	$assemblyDir = Split-Path -Parent $Path
	$assemblyFile = Split-Path -Leaf $Path
	$shadowCopyDirs = @("$env:temp\xrepo\1", "$env:temp\xrepo\2", "$env:temp\xrepo\3", "$env:temp\xrepo\4", "$env:temp\xrepo\5")
	$shadowCopyDir = $null
	for($i = 0; $i -lt $shadowCopyDirs.Length; $i++) {
		$shadowCopyDir = $shadowCopyDirs[$i]
		if(-not (Test-Path $shadowCopyDir)) {
			[IO.Directory]::CreateDirectory($shadowCopyDir)
		}
		if(-not (Test-FileLock $shadowCopyDir\$assemblyFile)) {
			break
		}
	}

	cp $assemblyDir\*.* $shadowCopyDir
	Add-Type -Path $shadowCopyDir\$assemblyFile
}

$LocalDevAssemblyLocation = Join-Path $ThisScriptPath "..\Core\bin\Debug\XRepo.Core.dll"
$DeployedAssemblyLocation = Join-Path $ThisScriptPath "..\tools\XRepo.Core.dll"

if(Test-Path $LocalDevAssemblyLocation) {
	Load-XRepoAssembly $LocalDevAssemblyLocation
}
else {
	Load-XRepoAssembly -Path $DeployedAssemblyLocation
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
	if($parts.Length -le 2) {
		return Get-XRepoCommands | Write-Expansions $lastWord
	}
	switch($parts[1]) {
		'pin' {
			Get-AssembliesAndRepos | Write-Expansions $lastWord
		}
		'unpin' {
			('all' + @(Get-AssembliesAndRepos)) | Write-Expansions $lastWord
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
		return TabExpansion_XRepoInner $line $lastWord
	}
	else {
		return $result
	}
}

function TabExpansion_XRepoInner($line, $lastWord) {
	return $null
}

function Install-XRepoTabExpansion {
	if(Test-Path function:TabExpansion) {
		cp function:TabExpansion function:TabExpansion_XRepoInner
	}
	else {
		#if a TabExpansion function doesn't exist, create it in the global scope
		function global:TabExpansion {}
	}
	cp function:TabExpansion_XRepoDecorator function:TabExpansion
	#cp function:XRepoTabExpansion function:TabExpansion
}
Export-ModuleMember Install-XRepoTabExpansion, TabExpansion_XRepoInner
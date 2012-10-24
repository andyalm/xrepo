param($tool = "all", $target = "c:\Tools\XRepo")

if($tool -eq 'build' -or $tool -eq 'all')
{
	$buildDir = "$target\build\"
	[IO.Directory]::CreateDirectory($buildDir)
	cp src\Build\bin\Debug\*.* $buildDir -r
}

if($tool -eq 'cmd' -or $tool -eq 'all')
{
	$binDir = "$target\bin\"
	[IO.Directory]::CreateDirectory($binDir)
	cp src\CommandLine\bin\Debug\*.* $binDir -r
}
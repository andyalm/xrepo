param($target = "c:\Tools\XRepo")

$buildDir = "$target\build\"
[IO.Directory]::CreateDirectory($buildDir)
cp src\Build\bin\Debug\*.* $buildDir -r

$binDir = "$target\bin\"
[IO.Directory]::CreateDirectory($binDir)
cp src\CommandLine\bin\Debug\*.* $binDir -r
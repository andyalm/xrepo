#!/bin/bash

sudo dotnet run --no-restore -p deploy/installer/XRepo.Installer.csproj uninstall

dotnet pack --no-restore
dotnet publish --no-restore src/Build/Build.csproj
sudo dotnet run --no-restore -p deploy/installer/XRepo.Installer.csproj install src/Build/bin/Debug/netstandard1.5/publish
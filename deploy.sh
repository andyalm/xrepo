#!/bin/bash

sudo dotnet run -p deploy/installer/XRepo.Installer.csproj uninstall

dotnet pack
dotnet publish src/Build/Build.csproj
sudo dotnet run -p deploy/installer/XRepo.Installer.csproj install src/Build/bin/Debug/netstandard1.5/publish
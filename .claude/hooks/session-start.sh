#!/bin/bash
set -euo pipefail

# Only run in remote (cloud) environments
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

# Install .NET 10 SDK if not already installed
if command -v dotnet &>/dev/null && dotnet --list-sdks 2>/dev/null | grep -q '^10\.'; then
  echo ".NET 10 SDK is already installed, skipping installation."
else
  echo "Installing .NET 10 SDK via apt..."

  # Add Microsoft package repository if not already present
  if [ ! -f /etc/apt/sources.list.d/microsoft-prod.list ]; then
    curl -sSL https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -o /tmp/packages-microsoft-prod.deb
    dpkg -i /tmp/packages-microsoft-prod.deb
    rm -f /tmp/packages-microsoft-prod.deb
  fi

  apt-get update -qq
  DEBIAN_FRONTEND=noninteractive apt-get install -y dotnet-sdk-10.0
fi

# Restore NuGet packages
echo "Restoring NuGet packages..."
dotnet restore "$CLAUDE_PROJECT_DIR/xrepo.sln"

echo "Session startup complete. .NET SDK version: $(dotnet --version)"

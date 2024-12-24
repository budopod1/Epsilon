#!/usr/bin/env bash
set -euo pipefail
cd "${0%/*}/.."

echo "Installing epslc..."

echo "Loading submodules..."
git submodule init
git submodule update --recursive --remote

echo "Building executable..."
./scripts/buildcs.bash
echo "Executable built"

echo "Setting up libraries..."
./scripts/buildlibs.bash

echo "Bootstrapping backend..."
./scripts/bootstrap.bash

echo "Completing setup..."

ln -fs ../../scripts/pre-commit.py .git/hooks/pre-commit

mkdir -p temp

echo
echo "Setup complete"
echo "Run \`sudo ./scripts/install.bash\` to install epslc"

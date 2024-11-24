#!/usr/bin/env bash
set -e
cd "${0%/*}"

echo "Setting up Epsilon (this can take a bit)..."

echo "Loading submodules..."
git submodule init
git submodule update --recursive --remote

echo "Building executable..."
./build.bash
echo "Executable built"

echo "Setting up libraries..."
./scripts/buildlibs.bash

echo "Bootstrapping backend..."
./scripts/bootstrap.bash

echo "Setting up tests..."
./tests/setup.bash

ln -fs ../../scripts/pre-commit.py .git/hooks/pre-commit

mkdir -p temp

echo "sudo required to add epslc as a command"
sudo ln -fs "$PWD/scripts/epslc.bash" /usr/bin/epslc || echo "Failed to add epslc as a command"

echo "Setup complete"

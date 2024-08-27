#!/usr/bin/env bash
set -e
cd "${0%/*}"

echo "Setting up Epsilon (this can take a bit)..."

# This is such a horrible solution
# https://stackoverflow.com/a/2705678
ESCAPED_PWD=$(printf '%s\n' "$PWD" | sed -e 's/[\/&]/\\&/g')
sed -i "s/.*DllImport.*/    [DllImport(\"$ESCAPED_PWD\/Utils\/runcommand.so\")]/g" Utils/CmdUtils.cs

echo "Loading submodules..."
git submodule init
git submodule update --recursive --remote

echo "Building executable..."
./build.bash
echo "Executable built"

echo "Setting up libraries..."
./scripts/buildlibs.bash

echo "Setting up tests..."
./tests/setup.bash

ln -fs ../../scripts/pre-commit.py .git/hooks/pre-commit

mkdir -p temp

sudo ln -fs "$PWD/scripts/epslc.bash" /usr/local/bin/epslc || echo "Failed to add Epsilon to /usr/local/bin"

echo "Setup complete"

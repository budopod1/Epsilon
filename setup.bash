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
./buildlibs.bash
echo "Setting up venv..."
rm -rdf venv
virtualenv venv
source venv/bin/activate
pip install --upgrade pip
pip install llvmlite
pip install orjson
python -c "import llvmlite;import orjson;"
echo "venv setup complete"
echo "Setting up tests..."
./tests/setup.bash
mkdir -p temp
echo "Setup complete"

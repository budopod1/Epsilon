#!/usr/bin/env bash
set -e
cd "${0%/*}"
echo "Setting up Epsilon (this can take a bit)..."
echo "Building executable..."
./build.bash
echo "Executable built"
echo "Setting up builtins..."
./buildbuiltins.bash
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
mkdir -p build
echo "Setup complete"

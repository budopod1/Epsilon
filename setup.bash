#!/usr/bin/env bash
set -e
cd "${0%/*}"
echo "Setting up builtins..."
./buildbuiltins.bash
echo "Builtins setup complete..."
echo "Setting up venv..."
rm -rdf venv
virtualenv venv
source venv/bin/activate
pip install --upgrade pip
pip install llvmlite
pip install orjson
python -c "import llvmlite;import orjson;"
echo "venv setup complete..."
echo "Setup complete"

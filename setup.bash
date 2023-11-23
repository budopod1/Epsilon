#!/usr/bin/env bash
set -e
cd "${0%/*}"
echo "Working..."
rm -rdf venv
virtualenv venv
source venv/bin/activate
pip install --upgrade pip
pip install llvmlite
pip install orjson
python -c "import llvmlite;import orjson;"
echo "Setup complete"

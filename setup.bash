set -e
echo "Working..."
rm -rdf venv
virtualenv venv
source venv/bin/activate
pip install --upgrade pip
pip install llvmlite
pip install orjson
python -c "import llvmlite;import orjson;print('Virtualenv setup complete');"
echo "Setup complete"

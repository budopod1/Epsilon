rm -rd venv 2> /dev/null
virtualenv venv
source venv/bin/activate
pip install --upgrade pip
pip install llvmlite
pip install orjson
python -c "import llvmlite;import orjson;print('\nVirtualenv setup complete')"

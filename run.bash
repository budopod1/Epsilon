rm App.exe 2> /dev/null
# find ./*/ -name "*.cs" -exec csc App.cs {} +
python combine.py
csc App.cs Combined.cs
if [ -f App.exe ]; then
    mono App.exe
fi

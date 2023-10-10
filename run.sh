rm App.exe 2> /dev/null
find ./*/ -name "*.cs" -exec csc -debug App.cs {} +
if [ -f App.exe ]; then
    mono --debug App.exe
fi

rm App.exe 2> /dev/null
csc App.cs ./**/*.cs
if [ -f App.exe ]
then
    mono App.exe
fi

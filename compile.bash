# required, so we don't run anything if compilation fails
rm App.exe 2> /dev/null

find ./*/ -name "*.cs" -exec csc -debug App.cs {} +

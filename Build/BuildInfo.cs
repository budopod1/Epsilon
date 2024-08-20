using System;
using System.Collections.Generic;

public class BuildInfo(string output, string outputName, IEnumerable<string> sources, FileTree entryFile, IEnumerable<FileTree> unlinkedFiles, string fileWithMain) {
    public string Output = output;
    public string OutputName = outputName;
    public IEnumerable<string> Sources = sources;
    public FileTree EntryFile = entryFile;
    public IEnumerable<FileTree> UnlinkedFiles = unlinkedFiles;
    public string FileWithMain = fileWithMain;
}

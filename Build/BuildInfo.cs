using System;
using System.Collections.Generic;

public class BuildInfo {
    public string Output;
    public string OutputName;
    public IEnumerable<string> Sources;
    public FileTree EntryFile;
    public IEnumerable<FileTree> UnlinkedFiles;
    public string FileWithMain;

    public BuildInfo(string output, string outputName, IEnumerable<string> sources, FileTree entryFile, IEnumerable<FileTree> unlinkedFiles, string fileWithMain) {
        Output = output;
        OutputName = outputName;
        Sources = sources;
        EntryFile = entryFile;
        UnlinkedFiles = unlinkedFiles;
        FileWithMain = fileWithMain;
    }
}

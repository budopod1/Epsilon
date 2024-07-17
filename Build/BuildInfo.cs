using System;
using System.Collections.Generic;

public class BuildInfo {
    public string Output;
    public string OutputName;
    public IEnumerable<string> Sources;
    public FileTree EntryFile;
    public IEnumerable<IClangConfig> ClangConfig;
    public IEnumerable<FileTree> UnlinkedFiles;
    public string FileWithMain;

    public BuildInfo(string output, string outputName, IEnumerable<string> sources, FileTree entryFile, IEnumerable<IClangConfig> clangConfig, IEnumerable<FileTree> unlinkedFiles, string fileWithMain) {
        Output = output;
        OutputName = outputName;
        Sources = sources;
        EntryFile = entryFile;
        ClangConfig = clangConfig;
        UnlinkedFiles = unlinkedFiles;
        FileWithMain = fileWithMain;
    }
}

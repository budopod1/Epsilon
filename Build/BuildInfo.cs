using System;
using System.Collections.Generic;

public class BuildInfo {
    public IEnumerable<string> Sources;
    public FileTree EntryFile;
    public IEnumerable<IClangConfig> ClangConfig;
    public IEnumerable<FileTree> UnlinkedFiles;
    public string FileWithMain;

    public BuildInfo(IEnumerable<string> sources, FileTree entryFile, IEnumerable<IClangConfig> clangConfig, IEnumerable<FileTree> unlinkedFiles, string fileWithMain) {
        Sources = sources;
        EntryFile = entryFile;
        ClangConfig = clangConfig;
        UnlinkedFiles = unlinkedFiles;
        FileWithMain = fileWithMain;
    }
}

using System;
using System.Collections.Generic;

public class BuildInfo {
    public FileTree EntryFile;
    public IEnumerable<IClangConfig> ClangConfig;
    public IEnumerable<FileTree> UnlinkedFiles;
    public string FileWithMain;

    public BuildInfo(FileTree entryFile, IEnumerable<IClangConfig> clangConfig, IEnumerable<FileTree> unlinkedFiles, string fileWithMain) {
        EntryFile = entryFile;
        ClangConfig = clangConfig;
        UnlinkedFiles = unlinkedFiles;
        FileWithMain = fileWithMain;
    }
}

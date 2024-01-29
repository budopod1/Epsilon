using System;

public class FileNotFoundErrorException : Exception {
    public string Path;
    
    public FileNotFoundErrorException(string path) {
        Path = path;
    }
}

using System;
using System.IO;

public class App {
    public static void Main(string[] args) {
        string filename = "code.mm";
        using (StreamReader file = new StreamReader(filename)) {
            Compiler compiler = new Compiler();
            compiler.Compile(file.ReadToEnd());
        }
    }
}

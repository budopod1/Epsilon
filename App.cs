using System;
using System.IO;

public class App {
    public static void Main(string[] args) {
        // File extension should be:
        // .ep, .epsilon, or .ε
        string filename = "code.ε";
        using (StreamReader file = new StreamReader(filename)) {
            Compiler compiler = new Compiler();
            compiler.Compile(file.ReadToEnd());
        }
    }
}

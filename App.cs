using System;
using System.IO;

public class App {
    public static void Main(string[] args) {
        // File extension should be:
        // .ep, .eps, .epsilon, or .ε
        string filename = "examples/iftest.ε";
        using (StreamReader file = new StreamReader(filename)) {
            Compiler compiler = new Compiler();
            compiler.Compile(filename, file.ReadToEnd());
        }
    }
}

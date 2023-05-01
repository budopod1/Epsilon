using System;
using System.Collections.Generic;

public class Compiler {
    public Compiler() {
        
    }

    public void Compile(string text) {
        Program program = new Program(new List<IToken>());
        foreach (char chr in text) {
            program.Add(new TextToken(chr.ToString()));
        }
        Console.WriteLine(program.ToString());
    }
}

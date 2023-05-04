using System;
using System.Collections.Generic;

public class Compiler {
    public Compiler() {}

    public void Compile(string text) {
        Console.WriteLine("Compiling...");
        Program program = new Program(new List<IToken>(), new Constants());
        foreach (char chr in text) {
            program.Add(new TextToken(chr.ToString()));
        }
        Console.WriteLine("Tokenizing strings...");
        program = TokenizeStrings(program);
        // Console.WriteLine(program.ToString());
        Console.WriteLine("Tokenizing names...");
        program = TokenizeNames(program);
        // Console.WriteLine(program.ToString());
        Console.WriteLine("Tokenizing floats...");
        program = TokenizeFloats(program);
        // Console.WriteLine(program.ToString());
        Console.WriteLine("Tokenizing ints...");
        program = TokenizeInts(program);
        Console.WriteLine(program.ToString());
    }

    public Program TokenizeStrings(Program program_) {
        Program program = program_;
        StringMatcher matcher = new StringMatcher();
        while (true) {
            Match match = matcher.match(program);
            if (match == null) break;
            string matchedString = String.Join("", match.GetMatched());
            int constant = program.GetConstants().AddConstant(
                new StringConstant(matchedString)
            );
            List<IToken> replacement = new List<IToken>();
            replacement.Add(new ConstantValue(constant));
            match.SetReplacement(replacement);
            program = (Program)match.Replace(program);
        }
        return program;
    }

    public Program TokenizeFloats(Program program_) {
        Program program = program_;
        FloatMatcher matcher = new FloatMatcher();
        while (true) {
            Match match = matcher.match(program);
            if (match == null) break;
            string matchedString = String.Join("", match.GetMatched());
            int constant = program.GetConstants().AddConstant(
                new FloatConstant(matchedString)
            );
            List<IToken> replacement = new List<IToken>();
            replacement.Add(new ConstantValue(constant));
            match.SetReplacement(replacement);
            program = (Program)match.Replace(program);
        }
        return program;
    }

    public Program TokenizeInts(Program program_) {
        Program program = program_;
        IntMatcher matcher = new IntMatcher();
        while (true) {
            Match match = matcher.match(program);
            if (match == null) break;
            string matchedString = String.Join("", match.GetMatched());
            int constant = program.GetConstants().AddConstant(
                new IntConstant(matchedString)
            );
            List<IToken> replacement = new List<IToken>();
            replacement.Add(new ConstantValue(constant));
            match.SetReplacement(replacement);
            program = (Program)match.Replace(program);
        }
        return program;
    }

    public Program TokenizeNames(Program program_) {
        Program program = program_;
        NameMatcher matcher = new NameMatcher();
        while (true) {
            Match match = matcher.match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }
}

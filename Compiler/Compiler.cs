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
        Console.WriteLine("Tokenizing function templates...");
        program = TokenizeFuncTemplates(program);
        // Console.WriteLine(program.ToString());
        Console.WriteLine("Tokenizing names...");
        program = TokenizeNames(program);
        // Console.WriteLine(program.ToString());
        Console.WriteLine("Tokenizing floats...");
        program = TokenizeFloats(program);
        // Console.WriteLine(program.ToString());
        Console.WriteLine("Tokenizing ints...");
        program = TokenizeInts(program);
        // Console.WriteLine(program.ToString());
        
        Console.WriteLine("Tokenizing symbols...");
        program = TokenizeSymbols(
            program,
            new Dictionary<string, Type> {
                {"=", typeof(Equal)},
                {" ", null},
                {"\n", null},
                {"\t", null},
                {"{", typeof(BracketOpen)},
                {"}", typeof(BracketClose)},
                {":", typeof(Colon)},
            }
        );
        
        Console.WriteLine("Tokenizing blocks...");
        program = TokenizeBlocks(program);
        
        Console.WriteLine("Tokenizing functions...");
        program = TokenizeFunctionHolders(program);
        
        Console.WriteLine("Tokenizing structs...");
        program = TokenizeStructHolders(program);
        
        Console.WriteLine(program.ToString());
    }

    public Program TokenizeFunctionHolders(Program program_) {
        Program program = program_;
        FunctionHolderMatcher matcher = new FunctionHolderMatcher(
            typeof(FuncTemplate), typeof(Block), typeof(FunctionHolder)
        );
        while (true) {
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }

    public Program TokenizeStructHolders(Program program_) {
        Program program = program_;
        StructHolderMatcher matcher = new StructHolderMatcher(
            typeof(Name), typeof(Block), typeof(StructHolder)
        );
        while (true) {
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }
    
    public Program TokenizeStrings(Program program_) {
        Program program = program_;
        StringMatcher matcher = new StringMatcher();
        while (true) {
            Match match = matcher.Match(program);
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
    
    public Program TokenizeFuncTemplates(Program program_) {
        Program program = program_;
        FuncTemplateMatcher matcher = new FuncTemplateMatcher(
            '#', '{', typeof(FuncTemplate)
        );
        while (true) {
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }

    public Program TokenizeBlocks(Program program_) {
        Program program = program_;
        BlockMatcher matcher = new BlockMatcher(
            typeof(BracketOpen), typeof(BracketClose),
            typeof(Block)
        );
        while (true) {
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }

    public Program TokenizeSymbols(Program program_, Dictionary<string, Type> symbols) {
        Program program = program_;
        SymbolMatcher matcher = new SymbolMatcher(symbols);
        while (true) {
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }
    
    public Program TokenizeFloats(Program program_) {
        Program program = program_;
        FloatMatcher matcher = new FloatMatcher();
        while (true) {
            Match match = matcher.Match(program);
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
            Match match = matcher.Match(program);
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
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }
}

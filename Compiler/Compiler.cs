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
        
        Console.WriteLine("Tokenizing function templates...");
        program = TokenizeFuncTemplates(program);
        
        Console.WriteLine("Tokenizing function arguments...");
        program = TokenizeFuncArguments(program);
        
        Console.WriteLine("Tokenizing names...");
        program = TokenizeNames(program);
        
        Console.WriteLine("Tokenizing floats...");
        program = TokenizeFloats(program);
        
        Console.WriteLine("Tokenizing ints...");
        program = TokenizeInts(program);
        
        Console.WriteLine("Tokenizing symbols...");
        program = TokenizeSymbols(program);
        
        Console.WriteLine("Tokenizing blocks...");
        program = TokenizeBlocks(program);
        
        Console.WriteLine("Tokenizing functions...");
        program = TokenizeFunctionHolders(program);
        
        Console.WriteLine("Tokenizing structs...");
        program = TokenizeStructHolders(program);

        List<string> baseTypes = ListBaseTypes_(program);
        
        Console.WriteLine("Tokenizing base types...");
        program = TokenizeBaseTypes_(program, baseTypes);
        
        Console.WriteLine("Tokenizing generics...");
        program = TokenizeGenerics(program);
        
        Console.WriteLine("Tokenizing types_...");
        program = TokenizeTypes_(program);
        
        Console.WriteLine("Tokenizing function argument types_...");
        program = TokenizeFuncArgumentTypes_(program, baseTypes);
        
        Console.WriteLine("Tokenizing var declarations...");
        program = TokenizeVarDeclarations(program);
        
        Console.WriteLine("Compiling structs...");
        program = CompileStructs(program);
        
        Console.WriteLine(program.ToString());
    }

    Program TokenizeFunctionHolders(Program program_) {
        Program program = program_;
        FunctionHolderMatcher matcher = new FunctionHolderMatcher(
            typeof(RawFuncTemplate), typeof(Block), typeof(FunctionHolder)
        );
        while (true) {
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }

    Program TokenizeFuncArguments(Program program_) {
        Program program = (Program)program_.Copy();
        IMatcher matcher = new FunctionArgumentMatcher(
            "<", ">", typeof(RawFunctionArgument)
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is RawFuncTemplate)) continue;
            RawFuncTemplate template = ((RawFuncTemplate)token);
            while (true) { 
                Match match = matcher.Match(template);
                if (match == null) break;
                template = (RawFuncTemplate)match.Replace(template);
            }
            program[i] = template;
        }
        return program;
    }

    Program TokenizeFuncArgumentTypes_(Program program_, List<string> bases) {
        Program program = (Program)program_.Copy();
        IMatcher symbolMatcher = new SymbolMatcher(
            new Dictionary<string, Type> {
                {"<", typeof(GenericsOpen)},
                {">", typeof(GenericsClose)}
            }
        );
        IMatcher nameMatcher = new NameMatcher();
        IMatcher baseMatcher = new UnitSwitcherMatcher<string>(
            typeof(Name), bases, typeof(BaseTokenType_)
        );
        IMatcher genericMatcher = new BlockMatcher(
            typeof(GenericsOpen), typeof(GenericsClose), typeof(Generics)
        );
        IMatcher type_Matcher = new Type_Matcher(
            typeof(BaseTokenType_), typeof(Generics), typeof(Type_Token),
            new ListTokenParser<Type_>(
                typeof(Comma), typeof(Type_Token), 
                (IToken generic) => ((Type_Token)generic).GetValue()
            )
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is FunctionHolder)) continue;
            FunctionHolder funcHolder = ((FunctionHolder)token);
            RawFuncTemplate template = funcHolder.GetRawTemplate();
            Console.WriteLine(template.ToString());
            for (int j = 0; j < template.Count; j++) {
                IToken subtoken = template[j];
                if (!(subtoken is RawFunctionArgument)) continue;
                TreeToken argument = ((TreeToken)subtoken);
                Console.WriteLine(argument.ToString());
                argument = PerformMatching(argument, symbolMatcher);
                argument = PerformMatching(argument, nameMatcher);
                argument = PerformMatching(argument, baseMatcher);
                argument = PerformTreeMatching(argument, genericMatcher);
                argument = PerformTreeMatching(argument, type_Matcher);
                Console.WriteLine(argument.ToString());
                template[j] = argument;
            }
        }
        return program;
    }

    Program TokenizeStructHolders(Program program_) {
        Program program = program_;
        StructHolderMatcher matcher = new StructHolderMatcher(
            typeof(Name), typeof(Block), typeof(StructHolder)
        );
        while (true) {
            // TODO: replace this block and similar ones in other 
            // functions with PerformMatching()
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }
    
    Program TokenizeStrings(Program program_) {
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
    
    Program TokenizeFuncTemplates(Program program_) {
        Program program = program_;
        RawFuncTemplateMatcher matcher = new RawFuncTemplateMatcher(
            '#', '{', typeof(RawFuncTemplate)
        );
        while (true) {
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }

    Program TokenizeBlocks(Program program_) {
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

    Program TokenizeSymbols(Program program_) {
        Program program = program_;
        SymbolMatcher matcher = new SymbolMatcher(
            new Dictionary<string, Type> {
                {"=", typeof(Equal)},
                {" ", null},
                {"\n", null},
                {"\t", null},
                {"{", typeof(BracketOpen)},
                {"}", typeof(BracketClose)},
                {"<", typeof(GenericsOpen)},
                {">", typeof(GenericsClose)},
                {":", typeof(Colon)},
                {",", typeof(Comma)},
            }
        );
        while (true) {
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }
    
    Program TokenizeFloats(Program program_) {
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

    Program TokenizeInts(Program program_) {
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

    Program TokenizeNames(Program program_) {
        Program program = program_;
        NameMatcher matcher = new NameMatcher();
        while (true) {
            Match match = matcher.Match(program);
            if (match == null) break;
            program = (Program)match.Replace(program);
        }
        return program;
    }

    Program TokenizeGenerics(Program program) {
        return (Program)PerformTreeMatching(program, new BlockMatcher(
            typeof(GenericsOpen), typeof(GenericsClose), typeof(Generics)
        ));
    }

    Program TokenizeBaseTypes_(Program program, List<string> bases) {
        program = (Program)program.Copy();
        foreach (IToken token in program) {
            if (token is Holder) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block, 
                    new UnitSwitcherMatcher<string>(
                        typeof(Name), bases, typeof(BaseTokenType_)
                    )
                );
                holder.SetBlock((Block)result);
            }
        }
        return program;
    }
    
    Program TokenizeTypes_(Program program) {
        program = (Program)program.Copy();
        foreach (IToken token in program) {
            if (token is Holder) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block, 
                    new Type_Matcher(
                        typeof(BaseTokenType_), typeof(Generics), typeof(Type_Token),
                        new ListTokenParser<Type_>(
                            typeof(Comma), typeof(Type_Token), 
                            (IToken generic) => ((Type_Token)generic).GetValue()
                        )
                    )
                );
                holder.SetBlock((Block)result);
            }
        }
        return program;
    }

    Program TokenizeVarDeclarations(Program program) {
        program = (Program)program.Copy();
        foreach (IToken token in program) {
            if (token is Holder) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block, 
                    new VarDeclareMatcher(
                        typeof(Name), typeof(Colon), typeof(Type_Token),
                        typeof(VarDeclaration)
                    )
                );
                holder.SetBlock((Block)result);
            }
        }
        return program;
    }

    Program CompileStructs(Program program) {
        return (Program)PerformMatching(
            program, new StructCompilerMatcher(
                typeof(StructHolder), typeof(Struct), 
                new ListTokenParser<Field>(
                    typeof(Comma), typeof(VarDeclaration), 
                    (token) => new Field((VarDeclaration)token)
                )
            )
        );
    }

    List<string> ListBaseTypes_(Program program) {
        List<string> types_ = new List<string>(Type_.BuiltInTypes_);
        foreach (IToken token_ in program) {
            if (token_ is StructHolder) {
                StructHolder token = ((StructHolder)token_);
                if (token.Count == 0) continue;
                IToken name = token[0];
                if (name is Name) {
                    types_.Add(((Name)name).GetValue());
                }
            }
        }
        return types_;
    }

    TreeToken PerformTreeMatching(TreeToken tree, IMatcher matcher) {
        tree = tree.Copy();
        tree = PerformMatching(tree, matcher);
        bool changed = false;

        do {
            (changed, tree) = PerformMatchingChanged(
                tree, matcher
            );
            foreach ((int i, TreeToken subtree) in tree.IndexTraverse()) {
                IToken subtoken = subtree[i];
                if (subtoken is TreeToken) {
                    (bool changedHere, TreeToken newToken) = PerformMatchingChanged(
                        (TreeToken)subtoken, matcher
                    );
                    subtree[i] = newToken;
                    if (changedHere) changed = true;
                }
            }
        } while (changed);
        
        return tree;
    }

    TreeToken PerformMatching(TreeToken tree, IMatcher matcher) {
        while (true) {
            Match match = matcher.Match(tree);
            if (match == null) break;
            tree = (TreeToken)match.Replace(tree);
        }
        return tree;
    }

    (bool, TreeToken) PerformMatchingChanged(TreeToken tree, IMatcher matcher) {
        bool changed = false;
        while (true) {
            Match match = matcher.Match(tree);
            if (match == null) break;
            changed = true;
            tree = (TreeToken)match.Replace(tree);
        }
        return (changed, tree);
    }
}

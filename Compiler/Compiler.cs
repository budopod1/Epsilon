using System;
using System.Linq;
using System.Collections.Generic;

public class Compiler {
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

        Console.WriteLine("Computing base types_...");
        ComputeBaseTypes_(program);
        
        Console.WriteLine("Tokenizing base types...");
        program = TokenizeBaseTypes_(program);
        
        Console.WriteLine("Tokenizing generics...");
        program = TokenizeGenerics(program);
        
        Console.WriteLine("Tokenizing types_...");
        program = TokenizeTypes_(program);
        
        Console.WriteLine("Tokenizing function argument types_...");
        program = TokenizeFuncArgumentTypes_(program);
        
        Console.WriteLine("Tokenizing var declarations...");
        program = TokenizeVarDeclarations(program);
        
        Console.WriteLine("Objectifying structs...");
        program = ObjectifyingStructs(program);

        Console.WriteLine("Tokenize template features...");
        program = TokenizeTemplateFeatures(program);
        
        Console.WriteLine("Parsing templates...");
        program = ParseFunctionTemplates(program);
        
        Console.WriteLine("Objectifying functions...");
        program = ObjectifyingFunctions(program);
        
        Console.WriteLine("Splitting program into lines...");
        program = SplitProgramIntoLines(program);
        
        Console.WriteLine("Getting scope variables...");
        program = GetScopeVariables(program);
        
        Console.WriteLine("Parsing function code...");
        program = ParseFunctionCode(program);
        
        Console.WriteLine(program.ToString());
    }

    Program ParseFunctionCode(Program program_) {
        Program program = ((Program)program_.Copy());

        program.UpdateParents();

        List<IMatcher> functionRules = new List<IMatcher>();
        List<IMatcher> addMatchingFunctionRules = new List<IMatcher>();

        List<Function> functions = new List<Function>();
        
        foreach (IToken token in program) {
            if (token is Function) {
                functions.Add((Function)token);
            }
        }

        foreach (Function function in functions) {
            functionRules.Add(
                new FunctionRuleMatcher(function, typeof(RawFunctionCall))
            );
            addMatchingFunctionRules.Add(
                new AddMatchingFunctionMatcher(function)
            );
        }
        
        List<List<IMatcher>> rules = new List<List<IMatcher>> {
            new List<IMatcher> {
                new BlockMatcher(
                    typeof(ParenthesisOpen), typeof(ParenthesisClose), typeof(RawGroup)
                ),
                new BlockMatcher(
                    typeof(SquareOpen), typeof(SquareClose), typeof(RawParameterGroup)
                )
            },
            functionRules,
            addMatchingFunctionRules,
            new List<IMatcher> {
                new GroupConverterMatcher(typeof(RawGroup), typeof(Group)),
                // new GroupConverterMatcher(typeof(RawParameterGroup), typeof(ParameterGroup)),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(RawFunctionCall))
                    }, new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                        RawFunctionCall call = ((RawFunctionCall)(tokens[0]));

                        List<Type_> paramTypes_ = new List<Type_>();
                        List<IValueToken> parameters = new List<IValueToken>();
                        
                        for (int i = 0; i < call.Count; i++) {
                            RawParameterGroup rparameter = (call[i]) as RawParameterGroup;
                            if (rparameter.Count != 1) return null;
                            IValueToken parameter = (rparameter[0]) as IValueToken;
                            if (parameter == null) return null;
                            paramTypes_.Add(parameter.GetType_());
                            parameters.Add(parameter);
                        }

                        foreach (Function function in call.GetMatchingFunctions()) {
                            List<Type_> argTypes_ = function.GetArguments().ConvertAll<Type_>(
                                (FunctionArgumentToken arg) => arg.GetType_()
                            );
                            if (paramTypes_.Count != argTypes_.Count) continue;

                            bool matches = true;

                            for (int i = 0; i < paramTypes_.Count; i++) {
                                Type_ pt = paramTypes_[i];
                                Type_ at = argTypes_[i];
                                if (!pt.IsConvertibleTo(at)) {
                                    matches = false;
                                    break;
                                }
                            }

                            if (matches) {
                                return new List<IToken> {
                                    new FunctionCall(
                                        function, parameters
                                    )
                                };
                            }
                        }
                        
                        return null;
                    })
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new ConditionPatternSegment<Name>(
                            (Name name) => Scope.GetEnclosing(name)
                                                .ContainsVar(name.GetValue())
                        ),
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0}), typeof(Variable)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(new Type_("Q")),
                        new TypePatternSegment(typeof(Plus)),
                        new Type_PatternSegment(new Type_("Q")),
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}), typeof(Addition)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(new Type_("Q")),
                        new TypePatternSegment(typeof(Minus)),
                        new Type_PatternSegment(new Type_("Q")),
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}), typeof(Subtraction)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(new Type_("Q")),
                        new TypePatternSegment(typeof(Star)),
                        new Type_PatternSegment(new Type_("Q")),
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}),
                                                 typeof(Multiplication)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(new Type_("Q")),
                        new TypePatternSegment(typeof(Slash)),
                        new Type_PatternSegment(new Type_("Q")),
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}), typeof(Division)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(new Type_("Q")),
                        new TypePatternSegment(typeof(Percent)),
                        new Type_PatternSegment(new Type_("Q")),
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}), typeof(Modulo)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Variable)),
                        new TypePatternSegment(typeof(Equal)),
                        new Type_PatternSegment(Type_.Any()),
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}), typeof(Assignment)
                    )
                ),
            }
        };
        
        foreach (Function function in functions) {
            Block block = function.GetBlock();
            function.SetBlock(DoBlockCodeRules(block, rules));
        }
        
        return program;
    }

    Block DoBlockCodeRules(Block block, List<List<IMatcher>> rules) {
        for (int i = 0; i < block.Count; i++) {
            IToken token = block[i];
            if (token is Line) {
                Line line = ((Line)token);
                foreach (List<IMatcher> ruleset in rules) {
                    line = (Line)DoTreeCodeRules(line, ruleset);
                }
                block[i] = line;
            } // TODO: else if (token is Block) {
        }
        return block;
    }

    IParentToken DoTreeCodeRules(IParentToken parent_, List<IMatcher> ruleset) {
        IParentToken parent = parent_;
        bool changed = true;
        while (changed) {
            changed = false;
            for (int i = 0; i < parent.Count; i++) {
                IToken sub = parent[i];
                if (sub is IParentToken) {
                    IParentToken subparent = ((IParentToken)sub);
                    parent[i] = DoTreeCodeRules(subparent, ruleset);
                }
            }
            if (parent is TreeToken) {
                TreeToken tree = ((TreeToken)parent);
                foreach (IMatcher rule in ruleset) {
                    (changed, parent) = PerformMatchingChanged(tree, rule);
                    if (changed) {
                        TokenUtils.UpdateParents(parent);
                        break;
                    }
                }
            }
        }
        return parent;
    }

    /*
    (IToken, bool) DoFunctionCodeRule(IParentToken parent_, IMatcher rule) {
        IParentToken parent = parent_;
        for (int i = 0; i < parent.Count; i++) {
            IToken sub = parent[i];
            if (sub is IParentToken) {
                bool thisChanged;
                (sub, thisChanged) = DoFunctionCodeRule((IParentToken)sub, rule);
                parent[i] = sub;
                if (thisChanged) {
                    return (parent, true);
                }
            }
        }
        bool changed = false;
        if (parent is TreeToken) {
            TreeToken tree = ((TreeToken)parent);
            (changed, parent) = PerformMatchingChanged(tree, rule);
            if (changed) TokenUtils.UpdateParents(parent);
        }
        return (parent, changed);
    }
    */

    Program SplitProgramIntoLines(Program program) {
        SplitTokensParser parser = new SplitTokensParser(typeof(HNL), false);
        foreach (IToken token in program) {
            if (token is Function) {
                Function function = ((Function)token);
                Block block = function.GetBlock();
                List<List<IToken>> rawLines = parser.Parse(block);
                List<IToken> lines = new List<IToken>();
                foreach(List<IToken> section in rawLines) {
                    lines.Add(new Line(section));
                }
                function.SetBlock((Block)block.Copy(lines));
            }
        }
        return program;
    }

    Program GetScopeVariables(Program program) {
        foreach (IToken token in program) {
            if (token is Function) {
                Function function = ((Function)token);
                Scope scope = function.GetScope();
                /*
                function.SetBlock((Block)PerformTreeMatching(
                    function.GetBlock(), new PatternMatcher(
                        new List<IPatternSegment> {
                            new TypePatternSegment(typeof(VarDeclaration)),
                        }, new DisposePatternProcessor((List<IToken> tokens) => {
                            VarDeclaration declaration = ((VarDeclaration)tokens[0]);
                            scope.AddVar(
                                declaration.GetName().GetValue(),
                                declaration.GetType_()
                            );
                        })
                    )
                ));
                */
                function.SetBlock((Block)PerformTreeMatching(
                    function.GetBlock(), new PatternMatcher(
                        new List<IPatternSegment> {
                            new TypePatternSegment(typeof(VarDeclaration)),
                        }, new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                            VarDeclaration declaration = ((VarDeclaration)tokens[0]);
                            string name = declaration.GetName().GetValue();
                            int id = scope.AddVar(
                                name, declaration.GetType_()
                            );
                            return new List<IToken> {
                                new Variable(name, id)
                            };
                        })
                    )
                ));
            }
        }
        return program;
    }

    Program ObjectifyingFunctions(Program program) {
        return (Program)PerformMatching(program, new FunctionObjectifyerMatcher(
            typeof(Function)
        ));
    }

    Program TokenizeTemplateFeatures(Program program_) {
        Program program = ((Program)program_.Copy());
        IMatcher symbolMatcher = new SymbolMatcher(new Dictionary<string, Type> {
            {" ", null}, {"\n", null}
        });
        IMatcher nameMatcher = new NameMatcher();
        IMatcher argumentConverterMatcher = new ArgumentConverterMatcher(
            typeof(RawFunctionArgument), typeof(Name), typeof(Type_Token),
            typeof(FunctionArgumentToken)
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is FunctionHolder)) continue;
            FunctionHolder holder = ((FunctionHolder)token);
            TreeToken template = holder.GetRawTemplate();
            template = PerformMatching(template, symbolMatcher);
            template = PerformMatching(template, nameMatcher);
            template = PerformMatching(template, argumentConverterMatcher);
            holder.SetTemplate(template);
        }
        return program;
    }

    Program ParseFunctionTemplates(Program program_) {
        Program program = ((Program)program_.Copy());
        List<Type> argumentTypes = new List<Type> {
            typeof(RawParameterGroup)
        };
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is FunctionHolder)) continue;
            FunctionHolder holder = ((FunctionHolder)token);
            RawFuncTemplate rawTemplate = holder.GetRawTemplate();
            List<IPatternSegment> segments = new List<IPatternSegment>();
            List<FunctionArgumentToken> arguments = new List<FunctionArgumentToken>();
            List<int> slots = new List<int>();
            int j = -1;
            foreach (IToken subtoken in rawTemplate) {
                j++;
                Type tokenType = subtoken.GetType();
                IPatternSegment segment = null;
                if (subtoken is TextToken) {
                    segment = new TextPatternSegment(
                        ((TextToken)subtoken).GetText()
                    );
                } else if (subtoken is Unit<string>) {
                    segment = new UnitPatternSegment<string>(
                        tokenType, ((Unit<string>)subtoken).GetValue()
                    );
                } else if (subtoken is FunctionArgumentToken) {
                    FunctionArgumentToken argument = ((FunctionArgumentToken)subtoken);
                    arguments.Add(argument);
                    segment = new TypesPatternSegment(argumentTypes);
                    slots.Add(j);
                }
                if (segment == null) {
                    throw new InvalidOperationException(
                        $"Segment of type {tokenType} cannot be part of a func template"
                    );
                }
                segments.Add(segment);
            }
            holder.SetTemplate(new FuncTemplate(
                new ConfigurablePatternExtractor<List<IToken>>(
                    segments, new SlotPatternProcessor(slots)
                ),
                arguments
            ));
        }
        return program;
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

    Program TokenizeFuncArgumentTypes_(Program program_) {
        Program program = (Program)program_.Copy();
        List<string> baseType_Names = program.GetBaseType_Names();
        Func<string, BaseType_> converter = (string source) => 
            BaseType_.ParseString(source, baseType_Names);
        IMatcher symbolMatcher = new SymbolMatcher(
            new Dictionary<string, Type> {
                {"<", typeof(GenericsOpen)},
                {">", typeof(GenericsClose)}
            }
        );
        IMatcher nameMatcher = new NameMatcher();
        IMatcher baseMatcher = new UnitSwitcherMatcher
                                   <string, BaseType_>(
            typeof(Name), converter, typeof(BaseTokenType_)
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
            for (int j = 0; j < template.Count; j++) {
                IToken subtoken = template[j];
                if (!(subtoken is RawFunctionArgument)) continue;
                TreeToken argument = ((TreeToken)subtoken);
                argument = PerformMatching(argument, symbolMatcher);
                argument = PerformMatching(argument, nameMatcher);
                argument = PerformMatching(argument, baseMatcher);
                argument = PerformTreeMatching(argument, genericMatcher);
                argument = PerformTreeMatching(argument, type_Matcher);
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
            string matchedString = String.Join(
                "", match.GetMatched().Select(
                    (IToken token) => ((TextToken)token).GetText()
                )
            );
            int constant = program.GetConstants().AddConstant(
                StringConstant.FromString(matchedString)
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
                {"+", typeof(Plus)},
                {"-", typeof(Minus)},
                {"*", typeof(Star)},
                {"/", typeof(Slash)},
                {"%", typeof(Percent)},
                {" ", null},
                {"\n", null},
                {"\t", null},
                {"{", typeof(BracketOpen)},
                {"}", typeof(BracketClose)},
                {"(", typeof(ParenthesisOpen)},
                {")", typeof(ParenthesisClose)},
                {"<", typeof(GenericsOpen)},
                {">", typeof(GenericsClose)},
                {"[", typeof(SquareOpen)},
                {"]", typeof(SquareClose)},
                {":", typeof(Colon)},
                {",", typeof(Comma)},
                {";", typeof(HNL)},
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
            string matchedString = String.Join(
                "", match.GetMatched().Select(
                    (IToken token) => ((TextToken)token).GetText()
                )
            );
            int constant = program.GetConstants().AddConstant(
                FloatConstant.FromString(matchedString)
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
            string matchedString = String.Join(
                "", match.GetMatched().Select(
                    (IToken token) => ((TextToken)token).GetText()
                )
            );
            int constant = program.GetConstants().AddConstant(
                IntConstant.FromString(matchedString)
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

    Program TokenizeBaseTypes_(Program program) {
        program = (Program)program.Copy();
        List<string> baseType_Names = program.GetBaseType_Names();
        Func<string, BaseType_> converter = (string source) => 
            BaseType_.ParseString(source, baseType_Names);
        foreach (IToken token in program) {
            if (token is Holder) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block, 
                    new UnitSwitcherMatcher<string, BaseType_>(
                        typeof(Name), converter,
                        typeof(BaseTokenType_)
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
                        typeof(BaseTokenType_), typeof(Generics),
                        typeof(Type_Token),
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

    Program ObjectifyingStructs(Program program) {
        return (Program)PerformMatching(
            program, new StructObjectifyerMatcher(
                typeof(StructHolder), typeof(Struct), 
                new ListTokenParser<Field>(
                    typeof(Comma), typeof(VarDeclaration), 
                    (token) => new Field((VarDeclaration)token)
                )
            )
        );
    }

    void ComputeBaseTypes_(Program program) {
        List<string> types_ = new List<string>();
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
        program.SetBaseType_Names(types_);
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

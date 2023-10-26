using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

public class Compiler {
    public bool DEBUG = false;
    public bool PRINT_RESULT = true;
    Stopwatch watch;
    
    public void Compile(string text) {
        try {
            _Compile(text);
        } catch (SyntaxErrorException e) {
            ShowCompilationError(e);
        } catch (TargetInvocationException e) {
            ShowCompilationError(e.InnerException);
        }
    }

    void ShowCompilationError(Exception e) {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("compilation error: ");
        Console.ResetColor();
        Console.WriteLine(e.Message);
    }

    void TimingStep() {
        if (DEBUG) {
            Console.WriteLine((watch.ElapsedMilliseconds/1000.0).ToString());
            watch.Restart();
        }
    }

    void _Compile(string text) {
        Console.WriteLine("Compiling...");
        
        Program program = new Program(new List<IToken>(), new Constants());
        foreach (char chr in text) {
            program.Add(new TextToken(chr.ToString()));
        }
        
        watch = new Stopwatch();
        if (DEBUG) watch.Start();

        Console.WriteLine("Tokenizing strings...");
        program = TokenizeStrings(program);
        TimingStep();

        Console.WriteLine("Removing comments...");
        program = RemoveComments(program);
        TimingStep();

        Console.WriteLine("Tokenizing function templates...");
        program = TokenizeFuncSignatures(program);
        TimingStep();

        Console.WriteLine("Tokenizing function arguments...");
        program = TokenizeFuncArguments(program);
        TimingStep();

        Console.WriteLine("Tokenizing names...");
        program = TokenizeNames(program);
        TimingStep();

        Console.WriteLine("Tokenizing keywords...");
        program = TokenizeKeywords(program);
        TimingStep();

        Console.WriteLine("Tokenizing floats...");
        program = TokenizeFloats(program);
        TimingStep();

        Console.WriteLine("Tokenizing ints...");
        program = TokenizeInts(program);
        TimingStep();

        Console.WriteLine("Removing whitespace...");
        program = RemoveWhitespace(program);
        TimingStep();

        Console.WriteLine("Tokenizing blocks...");
        program = TokenizeBlocks(program);
        TimingStep();

        Console.WriteLine("Tokenizing functions...");
        program = TokenizeFunctionHolders(program);
        TimingStep();

        Console.WriteLine("Tokenizing structs...");
        program = TokenizeStructHolders(program);
        TimingStep();

        Console.WriteLine("Converting function blocks...");
        program = ConvertFunctionBlocks(program);
        TimingStep();

        Console.WriteLine("Computing base types_...");
        ComputeBaseTypes_(program);
        TimingStep();

        Console.WriteLine("Tokenizing base types...");
        program = TokenizeBaseTypes_(program);
        TimingStep();

        Console.WriteLine("Tokenizing constant keyword values...");
        program = TokenizeConstantKeywordValues(program);
        TimingStep();

        Console.WriteLine("Tokenizing generics...");
        program = TokenizeGenerics(program);
        TimingStep();

        Console.WriteLine("Tokenizing types_...");
        program = TokenizeTypes_(program);
        TimingStep();

        Console.WriteLine("Tokenizing var declarations...");
        program = TokenizeVarDeclarations(program);
        TimingStep();

        Console.WriteLine("Objectifying structs...");
        program = ObjectifyingStructs(program);
        TimingStep();

        Console.WriteLine("Tokenize template features...");
        program = TokenizeTemplateFeatures(program);
        TimingStep();

        Console.WriteLine("Parsing templates...");
        program = ParseFunctionTemplates(program);
        TimingStep();

        Console.WriteLine("Parsing function signatures...");
        program = ParseFunctionSignatures(program);
        TimingStep();

        Console.WriteLine("Objectifying functions...");
        program = ObjectifyingFunctions(program);
        TimingStep();

        Console.WriteLine("Splitting program blocks into lines...");
        program = SplitProgramBlocksIntoLines(program);
        TimingStep();

        Console.WriteLine("Getting scope variables...");
        program = GetScopeVariables(program);
        TimingStep();

        Console.WriteLine("Parsing function code...");
        program = ParseFunctionCode(program);
        TimingStep();

        Console.WriteLine("Verifying code...");
        VerifyCode(program);
        TimingStep();
        
        if (PRINT_RESULT) Console.WriteLine(program);
    }
    
    Program TokenizeStrings(Program program) {
        return (Program)PerformMatching(program, new StringMatcher(program));
    }
    
    Program RemoveComments(Program program) {
        List<IToken> tokens = new List<IToken>();
        bool wasSlash = false;
        bool wasStar = false;
        bool isLineComment = false;
        bool isBlockComment = false;
        foreach (IToken token in program) {
            TextToken textT = token as TextToken;
            if (textT == null) {
                if (!isLineComment && !isBlockComment) tokens.Add(token);
            } else {
                string text = textT.GetText();
                if (isLineComment) {
                    if (text == "\n") isLineComment = false;
                } else if (isBlockComment) {
                    if (text == "*") {
                        wasStar = true;
                    }
                    if (wasStar && text == "/") {
                        isBlockComment = false;
                    }
                } else {
                    if (text == "/") {
                        if (wasSlash) {
                            isLineComment = true;
                            tokens.RemoveAt(tokens.Count-1);
                        } else {
                            wasSlash = true;
                            tokens.Add(token);
                        }
                    } else if (text == "*") {
                        if (wasSlash) {
                            isBlockComment = true;
                            tokens.RemoveAt(tokens.Count-1);
                        } else {
                            tokens.Add(token);
                        }
                    } else {
                        tokens.Add(token);
                    }
                }
                if (text != "/") wasSlash = false;
                if (text != "*") wasStar = false;
            }
        }
        return (Program)program.Copy(tokens);
    }
    
    Program TokenizeFuncSignatures(Program program) {
        return (Program)PerformMatching(program, new RawFuncSignatureMatcher());
    }

    Program TokenizeFuncArguments(Program program) {
        IMatcher matcher = new FunctionArgumentMatcher(
            "<", ">", typeof(RawFunctionArgument)
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is RawFuncSignature)) continue;
            RawFuncSignature sig = ((RawFuncSignature)token);
            RawFuncTemplate template = (RawFuncTemplate)sig.GetTemplate();
            sig.SetTemplate(PerformMatching(template, matcher));
        }
        return program;
    }

    Program TokenizeNames(Program program) {
        IMatcher matcher = new NameMatcher();
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is RawFuncSignature)) continue;
            RawFuncSignature sig = ((RawFuncSignature)token);
            sig.SetReturnType_(PerformMatching((TreeToken)sig.GetReturnType_(), matcher));
            sig.SetTemplate(PerformTreeMatching((TreeToken)sig.GetTemplate(), matcher));
        }
        return (Program)PerformMatching(program, matcher);
    }

    Program TokenizeKeywords(Program program) {
        Dictionary<string, Type> keywords = new Dictionary<string, Type> {
            {"return", typeof(ReturnKeyword)},
            {"if", typeof(IfKeyword)},
            {"else", typeof(ElseKeyword)},
            {"elif", typeof(ElseIfKeyword)},
            {"while", typeof(WhileKeyword)},
            {"switch", typeof(SwitchKeyword)},
        };
        return (Program)PerformMatching(
            program,
            new PatternMatcher(
                new List<IPatternSegment> {
                    new UnitsPatternSegment<string>(
                        typeof(Name), keywords.Keys.ToList()
                    )
                }, new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                    string name = ((Name)(tokens[0])).GetValue();
                    return new List<IToken> {(IToken)Activator.CreateInstance(
                        keywords[name]
                    )};
                })
            )
        );
    }
    
    Program TokenizeFloats(Program program) {
        return (Program)PerformMatching(program, new FloatMatcher(program));
    }

    Program TokenizeInts(Program program) {
        return (Program)PerformMatching(program, new IntMatcher(program));
    }

    Program RemoveWhitespace(Program program) {
        return (Program)program.Copy(program.GetTokens().Where(
            token => {
                if (token is TextToken) {
                    TextToken text = ((TextToken)token);
                    switch (text.GetText()) {
                        case " ":
                        case "\n":
                        case "\r":
                        case "\t":
                            return false;
                        default:
                            return true;
                    }
                }
                return true;
            }
        ).ToList());
    }

    Program TokenizeBlocks(Program program) {
        return (Program)PerformTreeMatching(program, new BlockMatcher(
            new TextPatternSegment("{"), new TextPatternSegment("}"),
            typeof(Block)
        ));
    }

    Program TokenizeFunctionHolders(Program program) {
        return (Program)PerformMatching(program, new PatternMatcher(
            new List<IPatternSegment> {
                new TypePatternSegment(typeof(RawFuncSignature)),
                new TypePatternSegment(typeof(Block))
            }, new WrapperPatternProcessor(typeof(FunctionHolder))
        ));
    }

    Program TokenizeStructHolders(Program program) {
        return (Program)PerformMatching(program, new PatternMatcher(
            new List<IPatternSegment> {
                new TypePatternSegment(typeof(Name)),
                new TypePatternSegment(typeof(Block))
            }, new WrapperPatternProcessor(typeof(StructHolder))
        ));
    }

    Program ConvertFunctionBlocks(Program program) {
        IMatcher matcher = new PatternMatcher(
            new List<IPatternSegment> {
                new TypePatternSegment(typeof(Block), true)
            }, new WrapperPatternProcessor(
                new UnwrapperPatternProcessor(),
                typeof(CodeBlock)
            )
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (token is FunctionHolder) {
                FunctionHolder holder = ((FunctionHolder)token);
                program[i] = PerformTreeMatching(holder, matcher);
            }
        }
        return program;
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

    Program TokenizeBaseTypes_(Program program) {
        List<string> baseType_Names = program.GetBaseType_Names();
        Func<string, BaseType_> converter = (string source) => 
            BaseType_.ParseString(source, baseType_Names);
        IMatcher matcher = new UnitSwitcherMatcher<string, BaseType_>(
            typeof(Name), converter, typeof(BaseTokenType_)
        );
        foreach (IToken token in program) {
            if (token is Holder) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block, matcher);
                holder.SetBlock((Block)result);
                if (token is FunctionHolder) {
                    RawFuncSignature sig = ((FunctionHolder)token).GetRawSignature();
                    sig.SetReturnType_(PerformTreeMatching(
                        (TreeToken)sig.GetReturnType_(), matcher)
                    );
                    TreeToken template = (TreeToken)sig.GetTemplate();
                    for (int i = 0; i < template.Count; i++) {
                        IToken sub = template[i];
                        if (sub is RawFunctionArgument) {
                            template[i] = PerformTreeMatching((RawFunctionArgument)sub, matcher);
                        }
                    }
                }
            }
        }
        return program;
    }
    
    Program TokenizeConstantKeywordValues(Program program) {
        Constants constants = program.GetConstants();
        Dictionary<string, Func<IConstant>> values = new Dictionary<string, Func<IConstant>> {
            {"true", () => new BoolConstant(true)},
            {"false", () => new BoolConstant(false)},
            {"infinity", () => new FloatConstant(Double.NegativeInfinity)},
            {"NaN", () => new FloatConstant(Double.NaN)},
            {"pi", () => new FloatConstant(MathF.PI)},
            {"e", () => new FloatConstant(MathF.E)},
        };
        IMatcher matcher = new PatternMatcher(
            new List<IPatternSegment> {
                new UnitsPatternSegment<string>(
                    typeof(Name), values.Keys.ToList()
                )
            }, new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                Name name = ((Name)tokens[0]);
                int constant = constants.AddConstant(
                    values[name.GetValue()]()
                );
                return new List<IToken> {new ConstantValue(constant)};
            })
        );
        foreach (IToken token in program) {
            if (token is FunctionHolder) {
                FunctionHolder holder = ((FunctionHolder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block, matcher);
                holder.SetBlock((Block)result);
            }
        }
        return program;
    }

    Program TokenizeGenerics(Program program) {
        IMatcher matcher = new BlockMatcher(
            new TypePatternSegment(typeof(BaseTokenType_)),
            new TextPatternSegment("<"), new TextPatternSegment(">"),
            typeof(Generics)
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is FunctionHolder)) continue;
            RawFuncSignature sig = ((FunctionHolder)token).GetRawSignature();
            sig.SetReturnType_(
                PerformTreeMatching((TreeToken)sig.GetReturnType_(), matcher)
            );
            sig.SetTemplate(
                PerformTreeMatching((TreeToken)sig.GetTemplate(), matcher)
            );
        }
        return (Program)PerformTreeMatching(program, matcher);
    }
    
    Program TokenizeTypes_(Program program) {
        IMatcher matcher = new Type_Matcher(
            typeof(BaseTokenType_), typeof(Generics), typeof(Type_Token),
            new ListTokenParser<Type_>(
                new TextPatternSegment(","), typeof(Type_Token), 
                (IToken generic) => ((Type_Token)generic).GetValue()
            )
        );
        foreach (IToken token in program) {
            if (token is Holder) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block, matcher);
                holder.SetBlock((Block)result);
                if (token is FunctionHolder) {
                    RawFuncSignature sig = ((FunctionHolder)token).GetRawSignature();
                    sig.SetReturnType_(PerformTreeMatching(
                        (TreeToken)sig.GetReturnType_(), matcher
                    ));
                    TreeToken template = (TreeToken)sig.GetTemplate();
                    for (int i = 0; i < template.Count; i++) {
                        IToken sub = template[i];
                        if (sub is RawFunctionArgument) {
                            template[i] = PerformTreeMatching(
                                (RawFunctionArgument)sub, matcher
                            );
                        }
                    }
                }
            }
        }
        return program;
    }

    Program TokenizeVarDeclarations(Program program) {
        foreach (IToken token in program) {
            if (token is Holder) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block,
                    new PatternMatcher(new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Type_Token)),
                        new TextPatternSegment(":"),
                        new TypePatternSegment(typeof(Name))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}), typeof(VarDeclaration)
                    ))
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
                    new TextPatternSegment(","), typeof(VarDeclaration), 
                    (token) => new Field((VarDeclaration)token)
                )
            )
        );
    }

    Program TokenizeTemplateFeatures(Program program) {
        IMatcher whitespaceMatcher = new PatternMatcher(
            new List<IPatternSegment> {
                new TextsPatternSegment(new List<string> {
                    " ", "\n", "\r", "\t"
                })
            }, new DisposePatternProcessor()
        );
        IMatcher argumentConverterMatcher = new ArgumentConverterMatcher(
            typeof(RawFunctionArgument), typeof(Name), typeof(Type_Token),
            typeof(FunctionArgumentToken)
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is FunctionHolder)) continue;
            FunctionHolder holder = ((FunctionHolder)token);
            RawFuncSignature sig = holder.GetRawSignature();
            TreeToken template = (TreeToken)sig.GetTemplate();
            template = PerformMatching(template, whitespaceMatcher);
            template = PerformMatching(template, argumentConverterMatcher);
            sig.SetTemplate(template);
        }
        return program;
    }

    Program ParseFunctionTemplates(Program program) {
        List<Type> argumentTypes = new List<Type> {
            typeof(RawSquareGroup)
        };
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is FunctionHolder)) continue;
            FunctionHolder holder = ((FunctionHolder)token);
            RawFuncSignature sig = holder.GetRawSignature();
            RawFuncTemplate rawTemplate = (RawFuncTemplate)sig.GetTemplate();
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
                    throw new SyntaxErrorException(
                        "Invalid syntax in function template"
                    );
                }
                segments.Add(segment);
            }
            sig.SetTemplate(new FuncTemplate(
                new ConfigurablePatternExtractor<List<IToken>>(
                    segments, new SlotPatternProcessor(slots)
                ),
                arguments
            ));
        }
        return program;
    }

    Program ParseFunctionSignatures(Program program) {
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is FunctionHolder)) continue;
            FunctionHolder holder = ((FunctionHolder)token);
            RawFuncSignature sig = holder.GetRawSignature();
            holder.SetSignature(
                new FuncSignature(
                    ((RawFuncReturnType_)sig.GetReturnType_()).GetType_(),
                    (FuncTemplate)sig.GetTemplate()
                )
            );
        }
        return program;
    }

    Program ObjectifyingFunctions(Program program) {
        return (Program)PerformMatching(program, new PatternMatcher(
            new List<IPatternSegment> {
                new TypePatternSegment(typeof(FunctionHolder))
            }, new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                FunctionHolder holder = ((FunctionHolder)tokens[0]);
                FuncSignature sig = holder.GetSignature();
                FuncTemplate template = sig.GetTemplate();
                return new List<IToken> {
                    new Function(
                        template.GetValue(), template.GetArguments(),
                        (CodeBlock)holder.GetBlock(), sig.GetReturnType_()
                    )
                };
            })
        ));
    }

    Program SplitProgramBlocksIntoLines(Program program) {
        foreach (IToken token in TokenUtils.Traverse(program)) {
            if (token is IParentToken) {
                IParentToken parent = ((IParentToken)token);
                for (int i = 0; i < parent.Count; i++) {
                    IToken sub = parent[i];
                    if (sub is CodeBlock) {
                        parent[i] = SplitBlockIntoLines((CodeBlock)sub);
                    }
                }
            }
        }
        return program;
    }

    CodeBlock SplitBlockIntoLines(CodeBlock block) {
        SplitTokensParser parser = new SplitTokensParser(
            new TextPatternSegment(";"), false
        );
        List<List<IToken>> rawLines = parser.Parse(block);
        if (rawLines == null) {
            throw new SyntaxErrorException("Unterminated block");
        }
        List<IToken> lines = new List<IToken>();
        foreach(List<IToken> section in rawLines) {
            lines.Add(new Line(section));
        }
        return (CodeBlock)block.Copy(lines);
    }

    Program GetScopeVariables(Program program) {
        foreach (IToken token in program) {
            if (token is Function) {
                Function function = ((Function)token);
                Scope scope = function.GetScope();
                function.SetBlock((CodeBlock)PerformTreeMatching(
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

    Program ParseFunctionCode(Program program) {
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
                    new TextPatternSegment("("), new TextPatternSegment(")"),
                    typeof(RawGroup)
                ),
                new BlockMatcher(
                    new TextPatternSegment("["), new TextPatternSegment("]"),
                    typeof(RawSquareGroup)
                ),
            },
            functionRules,
            addMatchingFunctionRules,
            new List<IMatcher> {
                new GroupConverterMatcher(typeof(RawGroup), typeof(Group)),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(RawFunctionCall))
                    }, new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                        RawFunctionCall call = ((RawFunctionCall)(tokens[0]));

                        List<Type_> paramTypes_ = new List<Type_>();
                        List<IValueToken> parameters = new List<IValueToken>();
                        
                        for (int i = 0; i < call.Count; i++) {
                            RawSquareGroup rparameter = (call[i]) as RawSquareGroup;
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
                        new ConditionPatternSegment<RawSquareGroup>(
                            (RawSquareGroup group) => !(group.parent is RawFunctionCall)
                        )
                    }, new WrapperPatternProcessor(
                        new SplitTokensProcessor(
                            new UnwrapperPatternProcessor(),
                            new TextPatternSegment(","),
                            typeof(ValueListItem)
                        ),
                        typeof(ValueList)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Type_Token)),
                        new TypePatternSegment(typeof(ValueList))
                    }, new Wrapper2PatternProcessor(
                        typeof(Instantiation)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new ConditionPatternSegment<Instantiation>(
                            i => i.GetType_().GetBaseType_().GetName() == "Array"
                        )
                    }, new Wrapper2PatternProcessor(
                        typeof(ArrayCreation)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new ConditionPatternSegment<Name>(
                            (Name name) => Scope.GetEnclosing(name)
                                                .ContainsVar(name.GetValue())
                        ),
                    }, new Wrapper2PatternProcessor(
                        typeof(Variable)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(IfKeyword)),
                        new TypePatternSegment(typeof(Group)),
                        new TypePatternSegment(typeof(CodeBlock))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {1, 2}),
                        typeof(Conditional)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Conditional)),
                        new TypePatternSegment(typeof(ElseIfKeyword)),
                        new TypePatternSegment(typeof(Group)),
                        new TypePatternSegment(typeof(CodeBlock))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2, 3}),
                        typeof(Conditional)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Conditional)),
                        new TypePatternSegment(typeof(ElseKeyword)),
                        new TypePatternSegment(typeof(CodeBlock))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}),
                        typeof(Conditional)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(WhileKeyword)),
                        new TypePatternSegment(typeof(Group)),
                        new TypePatternSegment(typeof(CodeBlock))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {1, 2}),
                        typeof(While)
                    )
                ),
                new AdvancedPatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(SwitchKeyword)),
                        new TypePatternSegment(typeof(Group))
                    }, new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Group)),
                        new TypePatternSegment(typeof(CodeBlock))
                    }, 1, -1, new List<IPatternSegment> {
                        new TypePatternSegment(typeof(CodeBlock))
                    }, 
                    new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                        return new List<IToken> {
                            new Switch((IValueToken)tokens[1], tokens.Skip(2).ToArray())
                        };
                    })
                ),
                new AdvancedPatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(SwitchKeyword)),
                        new TypePatternSegment(typeof(Group))
                    }, new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Group)),
                        new TypePatternSegment(typeof(CodeBlock))
                    }, 1, -1, new List<IPatternSegment>(), 
                    new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                        return new List<IToken> {
                            new Switch((IValueToken)tokens[1], tokens.Skip(2).ToArray())
                        };
                    })
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TextPatternSegment("!"),
                        new Type_PatternSegment(new Type_("Q"))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {1}),
                        typeof(Not)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TextPatternSegment("~"),
                        new Type_PatternSegment(new Type_("Z"))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {1}),
                        typeof(BitwiseNOT)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(new Type_("Q")),
                        new TextPatternSegment("*"),
                        new TextPatternSegment("*"),
                        new Type_PatternSegment(new Type_("Q"))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 3}),
                        typeof(Exponentiation)
                    )
                ),
                new CombinedMatchersMatcher(new List<IMatcher> {
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("*"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(Multiplication)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("/"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}), 
                            typeof(Division)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("%"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(Modulo)
                        )
                    ),
                }),
                new CombinedMatchersMatcher(new List<IMatcher> {
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("+"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(Addition)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("-"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(Subtraction)
                        )
                    ),
                }),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TextPatternSegment("-"),
                        new Type_PatternSegment(new Type_("Q"))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {1}),
                        typeof(Negation)
                    )
                ),
                new CombinedMatchersMatcher(new List<IMatcher> {
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Z")),
                            new TextPatternSegment(">"),
                            new TextPatternSegment(">"),
                            new Type_PatternSegment(new Type_("Z"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(BitshiftRight)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Z")),
                            new TextPatternSegment("<"),
                            new TextPatternSegment("<"),
                            new Type_PatternSegment(new Type_("Z"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(BitshiftLeft)
                        )
                    ),
                }),
                new CombinedMatchersMatcher(new List<IMatcher> {
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Z")),
                            new TextPatternSegment("&"),
                            new Type_PatternSegment(new Type_("Z"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(BitwiseAND)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Z")),
                            new TextPatternSegment("|"),
                            new Type_PatternSegment(new Type_("Z"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(BitwiseOR)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Z")),
                            new TextPatternSegment("^"),
                            new Type_PatternSegment(new Type_("Z"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(BitwiseXOR)
                        )
                    ),
                }),
                new CombinedMatchersMatcher(new List<IMatcher> {
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment(">"),
                            new Type_PatternSegment(Type_.Any())
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(Greater)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("<"),
                            new Type_PatternSegment(Type_.Any())
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(Less)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment(">"),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(Type_.Any())
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(GreaterEqual)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("<"),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(Type_.Any()),
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(LessEqual)
                        )
                    ),
                }),
                new CombinedMatchersMatcher(new List<IMatcher> {
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("="),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(Type_.Any())
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(Equals)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("!"),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(Type_.Any())
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(NotEquals)
                        )
                    ),
                }),
                new CombinedMatchersMatcher(new List<IMatcher> {
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("&"),
                            new TextPatternSegment("&"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(And)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("|"),
                            new TextPatternSegment("|"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(Or)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("^"),
                            new TextPatternSegment("^"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(Xor)
                        )
                    ),
                }),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Variable)),
                        new TextPatternSegment("="),
                        new Type_PatternSegment(Type_.Any())
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}),
                        typeof(Assignment)
                    )
                ),
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(ReturnKeyword)),
                        new Type_PatternSegment(Type_.Any())
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {1}),
                        typeof(Return)
                    )
                ),
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(ReturnKeyword))
                    }, new InstantiationPatternProcessor(
                        typeof(ReturnVoid)
                    )
                ),
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Group)),
                    }, new UnwrapperPatternProcessor()
                ),
            },
        };
        
        foreach (Function function in functions) {
            CodeBlock block = function.GetBlock();
            function.SetBlock(DoBlockCodeRules(block, rules));
        }
        
        return program;
    }

    CodeBlock DoBlockCodeRules(CodeBlock block, List<List<IMatcher>> rules) {
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
                if (sub is IParentToken && !(sub is IBarMatchingInto)) {
                    IParentToken subparent = ((IParentToken)sub);
                    parent[i] = DoTreeCodeRules(subparent, ruleset);
                }
            }
            if (parent is TreeToken) {
                TreeToken tree = ((TreeToken)parent);
                foreach (IMatcher rule in ruleset) {
                    Match match = rule.Match(tree);
                    if (match != null) {
                        changed = true;
                        parent = match.Replace(tree);
                        TokenUtils.UpdateParents(parent);
                        break;
                    }
                }
            } else {
                foreach (IMatcher rule in ruleset) {
                    Match match = rule.Match(parent);
                    if (match != null && match.Length() == 1) {
                        changed = true;
                        match.SingleReplace(parent);
                        TokenUtils.UpdateParents(parent);
                        break;
                    }
                }
            }
        }
        return parent;
    }

    TreeToken PerformTreeMatching(TreeToken tree, IMatcher matcher) {
        (bool _, IParentToken result) = PerformTreeMatching_(tree, matcher);
        return (TreeToken)result;
    }

    (bool, IParentToken) PerformTreeMatching_(IParentToken parent, IMatcher matcher) {
        bool changed = true;
        bool anyChanged = false;
        
        while (changed) {
            changed = false;
            
            for (int i = 0; i < parent.Count; i++) {
                IToken sub = parent[i];
                if (sub is IParentToken && !(sub is IBarMatchingInto)) {
                    IParentToken subparent = ((IParentToken)sub);
                    bool tchanged;
                    (tchanged, parent[i]) = PerformTreeMatching_(subparent, matcher);
                    changed |= tchanged;
                }
            }
            
            if (parent is TreeToken) {
                TreeToken tree = ((TreeToken)parent);
                bool tchanged;
                (tchanged, parent) = PerformMatchingChanged(tree, matcher);
                changed |= tchanged;
            } else {
                changed |= PerformIParentMatchingChanged(parent, matcher);
            }

            anyChanged |= changed;
        }
        
        return (anyChanged, parent);
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

    bool PerformIParentMatchingChanged(IParentToken parent, IMatcher matcher) {
        bool changed = false;
        while (true) {
            Match match = matcher.Match(parent);
            if (match == null || match.Length() != 1) break;
            changed = true;
            match.SingleReplace(parent);
        }
        return changed;
    }

    void VerifyCode(Program program) {
        foreach (IVerifier token in TokenUtils.TraverseFind<IVerifier>(program)) {
            token.Verify();
        }
    }
}

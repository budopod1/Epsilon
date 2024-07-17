using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

public class EPSLFileCompiler : IFileCompiler {
    Program program;
    string fileText;
    string srcPath;
    string IR;

    public EPSLFileCompiler(string path, string fileText) {
        Log.Info("Compiling EPSL file", path);
        srcPath = path;
        this.fileText = fileText;
        program = new Program(
            Utils.GetFullPath(path), new List<IToken>()
        );
        program.span = new CodeSpan(0, fileText.Length-1);
        int i = 0;
        foreach (char chr in fileText) {
            TextToken token = new TextToken(chr.ToString());
            token.span = new CodeSpan(i);
            program.Add(token);
            i++;
        }
    }

    public string GetText() {
        return fileText;
    }

    public string GetIDPath() {
        return srcPath;
    }

    public List<string> ToImports() {
        program = TokenizeStrings(program);

        program = TokenizeCharacterConstants(program);

        program = RemoveComments(program);

        program = TokenizeFuncSignatures(program);

        program = TokenizeNames(program);

        program = TokenizeImports(program);

        return program.Where(token => token is Import).Select(
            token => (token as Import).GetRealPath()
        ).ToList();
    }

    public HashSet<LocatedID> ToStructIDs() {
        program = TokenizeGlobals(program);

        program = TokenizeKeywords(program);

        program = TokenizeNumbers(program);

        program = RemoveWhitespace(program);

        program = TokenizeAnnotations(program);

        program = TokenizeBlocks(program);

        program = TokenizeFuncArguments(program);

        program = TokenizeFunctionHolders(program);

        program = TokenizeStructHolders(program);

        program = ConvertFunctionBlocks(program);

        ComputeBaseTypes_(program);

        return program.GetStructIDs();
    }

    public void AddStructIDs(HashSet<LocatedID> structIds) {
        program.AddStructIDs(structIds);
    }

    public List<RealFunctionDeclaration> ToDeclarations() {
        program = TokenizeBaseTypes_(program);

        program = TokenizeConstantKeywordValues(program);

        program = TokenizeGenerics(program);

        program = TokenizeTypes_(program);

        program = TokenizeVarDeclarations(program);

        program = ConvertTemplateArguments(program);

        program = ParseFunctionTemplates(program);

        program = ParseFunctionSignatures(program);

        program = JoinAnnotations(program);

        program = ObjectifyingFunctions(program);

        return program.Select(token => token as RealFunctionDeclaration)
            .Where(func => func != null).ToList();
    }

    public void AddDeclarations(List<RealFunctionDeclaration> declarations) {
        program.AddExternalDeclarations(declarations);
    }

    public HashSet<Struct> ToStructs() {
        program = ComputeStructs(program);

        return new HashSet<Struct>(program.GetStructsHere());
    }

    public void LoadStructExtendees() {
        program = LoadStructExtendeesHere(program);
    }

    public Dependencies ToDependencies(Builder builder) {
        program = VerifyPolyTypes_(program);

        program = SplitProgramBlocksIntoLines(program);

        program = ConvertStringLiterals(program);

        program = TokenizeGroups(program);

        program = TokenizeGivenBlocks(program);

        program = TokenizeForLoops(program);

        program = ParseGlobals(program);

        program = GetScopeVariables(program);

        program = ParseFunctionCode(program);

        VerifyCode(program);

        return GetDependencies(program);
    }

    public void FinishCompilation(string destPath) {
        AddUnusedValueWrappers(program);

        string json = GetJSONString(program);

        SaveJSON(json);

        CreateLLVMIR();

        IR = destPath + ".ll";
        File.Copy(Utils.JoinPaths(Utils.TempDir(), "code.ll"), IR, overwrite: true);
    }

    public string GetIR() {
        return IR;
    }

    public string GetObj() {
        return null;
    }

    public string GetSource() {
        return srcPath;
    }

    public bool FromCache() {
        return false;
    }

    public bool ShouldSaveSPEC() {
        return true;
    }

    public IEnumerable<IClangConfig> GetClangConfig() {
        return new List<IClangConfig>();
    }

    public FileSourceType GetFileSourceType() {
        return FileSourceType.User;
    }

    Program TokenizeStrings(Program program) {
        return (Program)PerformMatching(program, new StringMatcher(program));
    }

    Program TokenizeCharacterConstants(Program program) {
        Func<List<IToken>, List<IToken>> converter = tokens => {
            string txt = String.Join("", tokens.ConvertAll<string>(token=>token.ToString()));
            try {
                return new List<IToken> {new ConstantValue(CharConstant.FromString(txt))};
            } catch (OverflowException e) {
                throw new SyntaxErrorException(e.Message, TokenUtils.MergeSpans(tokens));
            }
        };
        Program program2 = (Program)PerformMatching(
            program, new PatternMatcher(new List<IPatternSegment> {
                new TextPatternSegment("'"),
                new TypePatternSegment(typeof(TextToken)),
                new TextPatternSegment("'")
            }, new FuncPatternProcessor<List<IToken>>(converter))
        );
        return (Program)PerformMatching(
            program2, new PatternMatcher(new List<IPatternSegment> {
                new TextPatternSegment("'"),
                new TextPatternSegment("\\"),
                new TypePatternSegment(typeof(TextToken)),
                new TextPatternSegment("'")
            }, new FuncPatternProcessor<List<IToken>>(converter))
        );
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

    Program TokenizeNames(Program program) {
        IMatcher matcher = new NameMatcher();
        foreach (IToken token in program) {
            if (!(token is RawFuncSignature)) continue;
            RawFuncSignature sig = ((RawFuncSignature)token);
            sig.SetReturnType_(PerformMatching((TreeToken)sig.GetReturnType_(), matcher));
            sig.SetTemplate(PerformMatching((TreeToken)sig.GetTemplate(), matcher));
        }
        return (Program)PerformMatching(program, matcher);
    }

    Program TokenizeImports(Program program) {
        return (Program)PerformMatching(program, new ImportMatcher());
    }

    Program TokenizeGlobals(Program program) {
        return (Program)PerformMatching(program, new GlobalsMatcher());
    }

    Program DoUnstructuredSyntaxMatching(Program program_, IMatcher matcher) {
        Program program = (Program)PerformMatching(program_, matcher);
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is RawGlobal)) continue;
            program[i] = PerformMatching((RawGlobal)token, matcher);
        }
        return program;
    }

    Program TokenizeKeywords(Program program) {
        Dictionary<string, Type> keywords = new Dictionary<string, Type> {
            {"return", typeof(ReturnKeyword)},
            {"if", typeof(IfKeyword)},
            {"else", typeof(ElseKeyword)},
            {"elif", typeof(ElseIfKeyword)},
            {"while", typeof(WhileKeyword)},
            {"switch", typeof(SwitchKeyword)},
            {"break", typeof(BreakKeyword)},
            {"continue", typeof(ContinueKeyword)},
            {"null", typeof(NullValue)},
            {"for", typeof(ForKeyword)},
            {"abort", typeof(AbortKeyword)},
            {"given", typeof(GivenKeyword)},
        };
        return DoUnstructuredSyntaxMatching(
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

    Program TokenizeNumbers(Program program) {
        return DoUnstructuredSyntaxMatching(program, new NumberMatcher(program));
    }

    List<IToken> RemoveWhiteSpaceFilter(IEnumerable<IToken> tokens) {
        return tokens.Where(
            token => {
                if (token is TextToken) {
                    TextToken text = ((TextToken)token);
                    return !Utils.Whitespace.Contains(text.GetText()[0]);
                }
                return true;
            }
        ).ToList();
    }

    Program RemoveWhitespace(Program program) {
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (token is RawFuncSignature) {
                RawFuncSignature sig = ((RawFuncSignature)token);
                RawFuncReturnType_ ret = (RawFuncReturnType_)sig.GetReturnType_();
                sig.SetReturnType_(
                    (RawFuncReturnType_)ret.Copy(
                        RemoveWhiteSpaceFilter(ret)
                    )
                );
                RawFuncTemplate template = (RawFuncTemplate)sig.GetTemplate();
                sig.SetTemplate(
                    (RawFuncTemplate)template.Copy(
                        RemoveWhiteSpaceFilter(template)
                    )
                );
            } else if (token is RawGlobal) {
                RawGlobal rawGlobal = (RawGlobal)token;
                program[i] = rawGlobal.Copy(RemoveWhiteSpaceFilter(rawGlobal));
            }
        }
        return (Program)program.Copy(RemoveWhiteSpaceFilter(program));
    }

    Program TokenizeAnnotations(Program program) {
        return (Program)PerformMatching(program, new AnnotationsMatcher());
    }

    Program TokenizeBlocks(Program program) {
        return (Program)PerformTreeMatching(program, new BlockMatcher(
            new TextPatternSegment("{"), new TextPatternSegment("}"),
            typeof(Block)
        ));
    }

    Program TokenizeFuncArguments(Program program) {
        IMatcher matcher = new FunctionArgumentMatcher();
        foreach (IToken token in program) {
            if (!(token is RawFuncSignature)) continue;
            RawFuncSignature sig = ((RawFuncSignature)token);
            RawFuncTemplate template = (RawFuncTemplate)sig.GetTemplate();
            sig.SetTemplate(PerformMatching(template, matcher));
        }
        return program;
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
                new TypePatternSegment(typeof(Block), exact: true)
            }, new FuncPatternProcessor<List<IToken>>(tokens => {
                return new List<IToken> {new CodeBlock(
                    program, ((Block)tokens[0]).GetTokens()
                )};
            })
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
        HashSet<LocatedID> structIds = new HashSet<LocatedID>();
        foreach (IToken token_ in program) {
            if (!(token_ is StructHolder)) continue;
            StructHolder token = ((StructHolder)token_);
            if (token.Count == 0) continue;
            IToken nameToken = token[0];
            if (!(nameToken is Name)) continue;
            string name = ((Name)nameToken).GetValue();
            structIds.Add(new LocatedID(GetIDPath(), name));
        }
        program.AddStructIDs(structIds);
    }

    Program DoStructuredSyntaxMatching(Program program, IMatcher matcher, bool matchIntoTemplates) {
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (token is RawGlobal) {
                program[i] = PerformTreeMatching((RawGlobal)token, matcher);
            } else if (token is Holder) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block, matcher);
                holder.SetBlock((Block)result);
                if (matchIntoTemplates && token is FunctionHolder) {
                    RawFuncSignature sig = ((FunctionHolder)token).GetRawSignature();
                    sig.SetReturnType_(PerformTreeMatching(
                        (TreeToken)sig.GetReturnType_(), matcher)
                    );
                    TreeToken template = (TreeToken)sig.GetTemplate();
                    for (int j = 0; j < template.Count; j++) {
                        IToken sub = template[j];
                        if (sub is RawFunctionArgument) {
                            template[j] = PerformTreeMatching((RawFunctionArgument)sub, matcher);
                        }
                    }
                }
            }
        }
        return program;
    }

    Program TokenizeBaseTypes_(Program program) {
        Func<string, UserBaseType_> converter = (string source) =>
            UserBaseType_.ParseString(source, program.GetStructIDs());
        return DoStructuredSyntaxMatching(
            program, new UnitSwitcherMatcher<string, UserBaseType_>(
                typeof(Name), converter, typeof(UserBaseType_Token)
            ), true
        );
    }

    Program TokenizeConstantKeywordValues(Program program) {
        Dictionary<string, Func<IConstant>> constantValues = new Dictionary<string, Func<IConstant>> {
            {"true", () => new BoolConstant(true)},
            {"false", () => new BoolConstant(false)},
            {"infinity", () => new FloatConstant(Double.NegativeInfinity)},
            {"NaN", () => new FloatConstant(Double.NaN)},
            {"pi", () => new FloatConstant(MathF.PI)},
        };
        return DoStructuredSyntaxMatching(
            program, new PatternMatcher(
                new List<IPatternSegment> {
                    new UnitsPatternSegment<string>(
                        typeof(Name), constantValues.Keys.ToList()
                    )
                }, new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                    string name = ((Name)tokens[0]).GetValue();
                    return new List<IToken> {
                        new ConstantValue(constantValues[name]())
                    };
                })
            ), false
        );
    }

    Program TokenizeGenerics(Program program) {
        return DoStructuredSyntaxMatching(
            program, new BlockMatcher(
                new TypePatternSegment(typeof(UserBaseType_Token)),
                new TextPatternSegment("<"), new TextPatternSegment(">"),
                typeof(Generics)
            ), true
        );
    }

    Program TokenizeTypes_(Program program) {
        Func<List<IToken>, Func<Type_>, List<IToken>> type_Wrapper = (tokens, inner) => {
            Type_ type_;
            CodeSpan span = TokenUtils.MergeSpans(tokens);
            try {
                type_ = inner();
            } catch (IllegalType_Exception e) {
                throw new SyntaxErrorException(e.Message, span);
            }
            program.AddParsedType_(span, type_);
            return new List<IToken> {new Type_Token(type_)};
        };

        return DoStructuredSyntaxMatching(
            program, new CombinedMatchersMatcher(new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TextPatternSegment("["),
                        new TypePatternSegment(typeof(Type_Token)),
                        new TextPatternSegment("]")
                    }, new FuncPatternProcessor<List<IToken>>(
                        tokens => type_Wrapper(tokens, () =>
                            ((Type_Token)tokens[1]).GetValue().ArrayOf())
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TextPatternSegment(":"),
                        new TypePatternSegment(typeof(Type_Token))
                    }, new FuncPatternProcessor<List<IToken>>(
                        tokens => type_Wrapper(tokens, () =>
                            ((Type_Token)tokens[1]).GetValue().PolyOf())
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Type_Token)),
                        new TextPatternSegment("?")
                    }, new FuncPatternProcessor<List<IToken>>(
                        tokens => type_Wrapper(tokens, () =>
                            ((Type_Token)tokens[0]).GetValue().OptionalOf())
                    )
                ),
                new Type_Matcher(type_Wrapper),
            }), true
        );
    }

    Program TokenizeVarDeclarations(Program program) {
        return DoStructuredSyntaxMatching(
            program, new PatternMatcher(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(Type_Token)),
                    new TextPatternSegment(":"),
                    new TypePatternSegment(typeof(Name))
                }, new Wrapper2PatternProcessor(
                    new SlotPatternProcessor(new List<int> {0, 2}),
                    typeof(VarDeclaration)
                )
            ), false
        );
    }

    Program ConvertTemplateArguments(Program program) {
        List<IPatternSegment> functionArgumentSegments = new List<IPatternSegment> {
            new TypePatternSegment(typeof(Type_Token)),
            new TextPatternSegment(":"),
            new TypePatternSegment(typeof(Name))
        };
        IMatcher argumentConverterMatcher = new PatternMatcher(
            new List<IPatternSegment> {
                new FuncPatternSegment<RawFunctionArgument>(arg => {
                    if (!TokenUtils.FullMatch(functionArgumentSegments, arg.GetTokens())) {
                        throw new SyntaxErrorException(
                            "Malformed function argument", arg
                        );
                    }
                    return true;
                })
            }, new FuncPatternProcessor<List<IToken>>(tokens => {
                RawFunctionArgument arg = (RawFunctionArgument)tokens[0];
                return new List<IToken> {
                    new FunctionArgumentToken(
                        ((Name)arg[2]).GetValue(), ((Type_Token)arg[0]).GetValue()
                    )
                };
            })
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (!(token is FunctionHolder)) continue;
            FunctionHolder holder = ((FunctionHolder)token);
            RawFuncSignature sig = holder.GetRawSignature();
            TreeToken template = (TreeToken)sig.GetTemplate();
            template = PerformMatching(template, argumentConverterMatcher);
            sig.SetTemplate(template);
        }
        return program;
    }

    Program ParseFunctionTemplates(Program program) {
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
                    segment = new FuncArgPatternSegment();
                    slots.Add(j);
                }
                if (segment == null) {
                    throw new SyntaxErrorException(
                        "Invalid syntax in function template", subtoken
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

    Program JoinAnnotations(Program program) {
        Program result = (Program)PerformMatching(program, new PatternMatcher(
            new List<IPatternSegment> {
                new TypePatternSegment(typeof(RawAnnotation)),
                new TypePatternSegment(typeof(IAnnotatable))
            }, new FuncPatternProcessor<List<IToken>>(tokens => {
                RawAnnotation rawAnnotation = ((RawAnnotation)tokens[0]);
                IAnnotation annotation = rawAnnotation.ToAnnotation();
                IAnnotatable annotatable = ((IAnnotatable)tokens[1]);
                if ((annotation.GetRecipients() & annotatable.RecipientType()) == 0) {
                    throw new SyntaxErrorException(
                        $"Cannot apply annotation of type '{rawAnnotation.AnnotationTypeName()}' to specified token", annotatable
                    );
                }
                annotatable.ApplyAnnotation(annotation);
                return new List<IToken> {annotatable};
            })
        ));
        IToken unmatchedRaw = result.FirstOrDefault(token => token is RawAnnotation);
        if (unmatchedRaw != null) {
            throw new SyntaxErrorException("Unmatched annotation", unmatchedRaw);
        }
        return result;
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
                        GetIDPath(), program, template.GetValue(), template.GetArguments(),
                        (CodeBlock)holder.GetBlock(), sig.GetReturnType_(), holder.GetAnnotations()
                    )
                };
            })
        ));
    }

    Program ComputeStructs(Program program) {
        ListTokenParser<Field> listParser = new ListTokenParser<Field>(
            new TextPatternSegment(","), typeof(VarDeclaration),
            (token) => new Field((VarDeclaration)token)
        );
        HashSet<Struct> structs = new HashSet<Struct>();
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (token is StructHolder) {
                StructHolder holder = ((StructHolder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                IToken nameT = holder[0];
                if (!(nameT is Unit<string>)) continue;
                Name name = ((Name)nameT);
                string nameStr = name.GetValue();
                List<Field> fields = listParser.Parse(block);
                if (fields == null) {
                    throw new SyntaxErrorException(
                        "Malformed struct", token
                    );
                }
                structs.Add(new Struct(GetIDPath(), nameStr, fields,
                    holder.GetAnnotations()));
            }
        }
        program.SetStructsHere(structs);
        return (Program)program.Copy(program.Where(token => !(token is StructHolder)).ToList());
    }

    Program LoadStructExtendeesHere(Program program) {
        foreach (Struct struct_ in program.GetStructsHere()) {
            struct_.LoadExtendee(program);
        }
        return program;
    }

    Program VerifyPolyTypes_(Program program) {
        foreach ((CodeSpan span, Type_ type_) in program.ListParsedTypes_()) {
            try {
                type_.VerifyValidPoly(recursive: false);
            } catch (IllegalType_Exception e) {
                throw new SyntaxErrorException(e.Message, span);
            }
        }
        return program;
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
            throw new SyntaxErrorException("Missing semicolon", block);
        }
        List<IToken> lines = new List<IToken>();
        foreach(List<IToken> section in rawLines) {
            Line line = new Line(section);
            line.span = TokenUtils.MergeSpans(section);
            lines.Add(line);
        }
        return (CodeBlock)block.Copy(lines);
    }

    Program ConvertStringLiterals(Program program) {
        return (Program)PerformTreeMatching(
            program, new PatternMatcher(
                new List<IPatternSegment> {
                    new FuncPatternSegment<ConstantValue>(
                        constant => constant.GetValue() is StringConstant
                    )
                }, new FuncPatternProcessor<List<IToken>>(tokens => {
                    ConstantValue sval = (ConstantValue)tokens[0];
                    string text = (((StringConstant)sval.GetValue()).GetValue());
                    return new List<IToken> {new StringLiteral(text)};
                })
            )
        );
    }

    Program TokenizeGroups(Program _program) {
        List<IMatcher> matchers = new List<IMatcher> {
            new PatternMatcher(
                new List<IPatternSegment> {
                    new TextPatternSegment("("),
                    new TypePatternSegment(typeof(Type_Token)),
                    new TextPatternSegment(")")
                }, new Wrapper2PatternProcessor(
                    new SlotPatternProcessor(new List<int> {1}),
                    typeof(UnmatchedCast)
                )
            ),
            new BlockMatcher(
                new TextPatternSegment("["), new TextPatternSegment("]"),
                typeof(RawSquareGroup)
            ),
            new BlockMatcher(
                new TextPatternSegment("("), new TextPatternSegment(")"),
                typeof(RawGroup)
            )
        };
        Program program = _program;
        foreach(IMatcher matcher in matchers) {
            program = (Program)PerformTreeMatching(program, matcher);
        }
        return program;
    }

    Program TokenizeForLoops(Program program) {
        return (Program)PerformTreeMatching(
            program, new PatternMatcher(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(ForKeyword)),
                    new TypePatternSegment(typeof(RawGroup)),
                    new TypePatternSegment(typeof(CodeBlock))
                }, new FuncPatternProcessor<List<IToken>>(tokens => {
                    RawGroup condition = (RawGroup)tokens[1];
                    if (condition.Count <= 1) {
                        throw new SyntaxErrorException(
                            "For loop condition cannot be empty and must have at least one clause", tokens[1]
                        );
                    }
                    return new List<IToken> {new RawFor(
                        condition.GetTokens(), (CodeBlock)tokens[2]
                    )};
                })
            )
        );
    }

    Program TokenizeGivenBlocks(Program program) {
        return (Program)PerformTreeMatching(
            program, new PatternMatcher(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(GivenKeyword)),
                    new TypePatternSegment(typeof(RawGroup)),
                    new TypePatternSegment(typeof(CodeBlock))
                }, new FuncPatternProcessor<List<IToken>>(tokens => {
                    RawGroup group = ((RawGroup)tokens[1]);
                    int asIdx = -1;
                    for (int i = 0; i < group.Count; i++) {
                        IToken token = group[i];
                        Name name = token as Name;
                        if (name != null && name.GetValue() == "as") {
                            asIdx = i;
                            break;
                        }
                    }
                    if (asIdx == -1) {
                        throw new SyntaxErrorException(
                            "Expected 'as' keyword", group
                        );
                    }
                    if (asIdx == group.Count - 1) {
                        throw new SyntaxErrorException(
                            "Expected variable declaration after 'as' keyword", group[asIdx]
                        );
                    }
                    if (asIdx < group.Count - 2) {
                        throw new SyntaxErrorException(
                            "Expected only a variable declaration after 'as' keyword", group[group.Count - 1]
                        );
                    }
                    IToken endingToken = group[group.Count - 1];
                    VarDeclaration var_ = endingToken as VarDeclaration;
                    if (var_ == null) {
                        throw new SyntaxErrorException(
                            "Expected variable declaration", endingToken
                        );
                    }
                    RawGivenValue givenValue = new RawGivenValue(group.GetTokens().Slice(asIdx));
                    CodeBlock block = ((CodeBlock)tokens[2]);
                    return new List<IToken> {new RawGivenPart(
                        givenValue, var_, block
                    )};
                })
            )
        );
    }

    Program ParseGlobals(Program program) {
        (IEnumerable<IToken> progTokens, IEnumerable<RawGlobal> globals) = program.ParitionSubclass<IToken, RawGlobal>();
        program.AddGlobals(globals.Select(rawGlobal => new Global(rawGlobal)));
        return (Program)program.Copy(progTokens.ToList());
    }

    Program GetScopeVariables(Program program) {
        program.UpdateParents();

        foreach (IToken token in program) {
            if (token is Function) {
                Function function = ((Function)token);
                foreach (VarDeclaration declaration
                        in TokenUtils.TraverseFind<VarDeclaration>(function)) {
                    string name = declaration.GetName().GetValue();
                    IScope scope = Scope.GetEnclosing(declaration);
                    declaration.SetID(scope.AddVar(
                        name, declaration.GetType_()
                    ));
                }
            }
        }

        return program;
    }

    Program ParseFunctionCode(Program program) {
        List<IMatcher> functionRules = new List<IMatcher>();
        List<IMatcher> addMatchingFunctionRules = new List<IMatcher>();

        List<FunctionDeclaration> functions = new List<FunctionDeclaration>();
        functions.AddRange(BuiltinsList.Builtins);
        functions.AddRange(program.GetExternalDeclarations());

        foreach (IToken token in program) {
            if (token is Function) {
                functions.Add((Function)token);
            }
        }

        functions.Sort();

        List<PatternExtractor<List<IToken>>> extractors = new List<PatternExtractor<List<IToken>>>();
        foreach (FunctionDeclaration function in functions) {
            PatternExtractor<List<IToken>> extractor = function.GetPattern();
            bool unique = true;
            foreach (PatternExtractor<List<IToken>> oextractor in extractors) {
                if (oextractor.Equals(extractor)) unique = false;
            }
            if (unique) {
                extractors.Add(extractor);
                functionRules.Add(new FunctionRuleMatcher(extractor));
            }
        }

        foreach (FunctionDeclaration function in functions) {
            addMatchingFunctionRules.Add(
                new AddMatchingFunctionMatcher(function)
            );
        }

        Dictionary<string, Type> floatCompoundableOperators = new Dictionary<string, Type> {
            {"+", typeof(Addition)}, {"-", typeof(Subtraction)},
            {"*", typeof(Multiplication)}, {"/", typeof(Division)},
            {"%", typeof(Modulo)}
        };

        List<List<IMatcher>> rules = new List<List<IMatcher>> {
            functionRules,
            addMatchingFunctionRules,
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TextPatternSegment("."),
                        new TypePatternSegment(typeof(Name))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {1}),
                        typeof(MemberAccessPostfix)
                    )
                )
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new FuncPatternSegment<Name>(
                            (Name name) => Scope.ContainsVar(name, name.GetValue())
                        ),
                    }, new Wrapper2PatternProcessor(
                        typeof(Variable)
                    )
                )
            },
            new List<IMatcher> {
                new GroupConverterMatcher(typeof(RawGroup), typeof(Group)),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(RawFunctionCall))
                    }, new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                        RawFunctionCall rawCall = ((RawFunctionCall)(tokens[0]));

                        List<Type_> paramTypes_ = new List<Type_>();
                        List<IValueToken> parameters = new List<IValueToken>();

                        for (int i = 0; i < rawCall.Count; i++) {
                            RawSquareGroup rparameter = (rawCall[i]) as RawSquareGroup;
                            if (rparameter.Count == 0) {
                                throw new SyntaxErrorException(
                                    "Function parameters cannot be empty", rparameter
                                );
                            }
                            IValueToken parameter = (rparameter[0]) as IValueToken;
                            if (parameter == null || rparameter.Count > 1) {
                                throw new SyntaxErrorException(
                                    "Illegal syntax in function parameter", rparameter
                                );
                            }
                            paramTypes_.Add(parameter.GetType_());
                            parameters.Add(parameter);
                        }

                        foreach (FunctionDeclaration function in rawCall.GetMatchingFunctions()) {
                            List<FunctionArgument> args = function.GetArguments();
                            if (paramTypes_.Count != args.Count) continue;

                            bool matches = true;

                            for (int i = 0; i < paramTypes_.Count; i++) {
                                Type_ paramType_ = paramTypes_[i];
                                FunctionArgument arg = args[i];
                                if (!arg.IsCompatibleWith(paramType_)) {
                                    matches = false;
                                    break;
                                }
                            }

                            if (matches) {
                                IFunctionCall call;
                                if (function.DoesReturnVoid()) {
                                    call = new VoidFunctionCall(function, parameters);
                                } else {
                                    call = new FunctionCall(function, parameters);
                                }
                                return new List<IToken> {call};
                            }
                        }

                        throw new SyntaxErrorException(
                            "Types supplied to function do not match any overload", rawCall
                        );
                    })
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new FuncPatternSegment<RawSquareGroup>(
                            (RawSquareGroup group) => !(group.parent is RawFunctionCall)
                        )
                    }, new WrapperPatternProcessor(
                        new SplitTokensPatternProcessor(
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
                        new FuncPatternSegment<Instantiation>(
                            i => i.GetType_().GetBaseType_().GetName() == "Array"
                        )
                    }, new Wrapper2PatternProcessor(
                        typeof(ArrayCreation)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(Type_.Any().ArrayOf()),
                        new TypePatternSegment(typeof(ValueList)),
                        new TextPatternSegment("?")
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 1}),
                        typeof(OptionalArrayAccess)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(Type_.Any().ArrayOf()),
                        new TypePatternSegment(typeof(ValueList))
                    }, new Wrapper2PatternProcessor(
                        typeof(ArrayAccess)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(Type_.Any()),
                        new TypePatternSegment(typeof(MemberAccessPostfix))
                    }, new Wrapper2PatternProcessor(
                        typeof(MemberAccess)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(BreakKeyword))
                    }, new InstantiationPatternProcessor(typeof(Break))
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(ContinueKeyword))
                    }, new InstantiationPatternProcessor(typeof(Continue))
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Given)),
                        new TypePatternSegment(typeof(RawGivenPart))
                    }, new Wrapper2PatternProcessor(
                        typeof(Given)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(RawGivenPart))
                    }, new Wrapper2PatternProcessor(
                        typeof(Given)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Given)),
                        new TypePatternSegment(typeof(ElseKeyword)),
                        new TypePatternSegment(typeof(CodeBlock))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}),
                        typeof(Given)
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
                        new TypePatternSegment(typeof(RawFor))
                    }, new Wrapper2PatternProcessor(
                        typeof(For)
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
                        new Type_PatternSegment(Type_.Any())
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {1}),
                        typeof(Not)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(Type_Token)),
                        new TextPatternSegment("*"),
                        new Type_PatternSegment(new Type_("W"))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}),
                        typeof(ZeroedArrayCreation)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(UnmatchedCast)),
                        new Type_PatternSegment(Type_.Any())
                    }, new Wrapper2PatternProcessor(typeof(Cast))
                ),
                new CombinedMatchersMatcher(new List<IMatcher> {
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new AndPatternSegment(
                                new Type_PatternSegment(new Type_("Q")),
                                new TypePatternSegment(typeof(IAssignableValue))
                            ),
                            new TextPatternSegment("+"),
                            new TextPatternSegment("+")
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0}),
                            typeof(PostIncrement)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new AndPatternSegment(
                                new Type_PatternSegment(new Type_("Q")),
                                new TypePatternSegment(typeof(IAssignableValue))
                            ),
                            new TextPatternSegment("-"),
                            new TextPatternSegment("-")
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0}),
                            typeof(PostDecrement)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new TextPatternSegment("+"),
                            new TextPatternSegment("+"),
                            new AndPatternSegment(
                                new Type_PatternSegment(new Type_("Q")),
                                new TypePatternSegment(typeof(IAssignableValue))
                            )
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {2}),
                            typeof(PreIncrement)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new TextPatternSegment("-"),
                            new TextPatternSegment("-"),
                            new AndPatternSegment(
                                new Type_PatternSegment(new Type_("Q")),
                                new TypePatternSegment(typeof(IAssignableValue))
                            )
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {2}),
                            typeof(PreDecrement)
                        )
                    )
                }),
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
                            new Type_PatternSegment(new Type_("Z")),
                            new TextPatternSegment("~"),
                            new TextPatternSegment("/"),
                            new Type_PatternSegment(new Type_("Z"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(IntDivision)
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
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new Type_PatternSegment(Type_.String()),
                        new TextPatternSegment("%"),
                        new Type_PatternSegment(Type_.Any())
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}),
                        typeof(FormatChain)
                    )
                ),
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(FormatChain)),
                        new TextPatternSegment("%"),
                        new Type_PatternSegment(Type_.Any())
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}),
                        typeof(FormatChain)
                    )
                ),
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
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TypePatternSegment(typeof(Negation))
                        }, new FuncPatternProcessor<List<IToken>>(tokens => new List<IToken> {
                            new Subtraction(
                                (IValueToken)tokens[0], ((Negation)tokens[1]).Sub()
                            )
                        })
                    ),
                }),
                new CombinedMatchersMatcher(new List<IMatcher> {
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment(">"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(Greater)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("<"),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 2}),
                            typeof(Less)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment(">"),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(new Type_("Q"))
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(GreaterEqual)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(new Type_("Q")),
                            new TextPatternSegment("<"),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(new Type_("Q")),
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
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("&"),
                            new TextPatternSegment("&"),
                            new Type_PatternSegment(Type_.Any())
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(And)
                        )
                    ),
                    new PatternMatcher(
                        new List<IPatternSegment> {
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("|"),
                            new TextPatternSegment("|"),
                            new Type_PatternSegment(Type_.Any())
                        }, new Wrapper2PatternProcessor(
                            new SlotPatternProcessor(new List<int> {0, 3}),
                            typeof(Or)
                        )
                    ),
                }),
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(VarDeclaration)),
                        new TextPatternSegment("="),
                        new Type_PatternSegment(Type_.Any())
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0, 2}),
                        typeof(InitialAssignment)
                    )
                )
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(VarDeclaration))
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {0}),
                        typeof(UninitVarDeclaration)
                    )
                )
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(IAssignableValue)),
                        new TextPatternSegment("="),
                        new Type_PatternSegment(Type_.Any())
                    }, new FuncPatternProcessor<List<IToken>>(tokens => new List<IToken> {
                        ((IAssignableValue)tokens[0]).AssignTo(
                            (IValueToken)tokens[2]
                        )
                    })
                )
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(IAssignableValue)),
                        new TextsPatternSegment(
                            floatCompoundableOperators.Keys.ToList()
                        ),
                        new TextPatternSegment("="),
                        new Type_PatternSegment(new Type_("Q"))
                    }, new FuncPatternProcessor<List<IToken>>(tokens => new List<IToken> {
                        new CompoundAssignment(
                            floatCompoundableOperators[
                                ((TextToken)tokens[1]).GetText()
                            ], (IAssignableValue)tokens[0], (IValueToken)tokens[3]
                        )
                    })
                )
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new FuncPatternSegment<Division>(division => (
                            (division[0] is ConstantValue)
                            && (division[1] is ConstantValue)
                            && (((ConstantValue)division[0]).GetValue() is INumberConstant)
                            && (((ConstantValue)division[1]).GetValue() is INumberConstant)
                        ))
                    }, new FuncPatternProcessor<List<IToken>>(tokens => new List<IToken> {
                        new ConstantValue(
                            new FloatConstant(
                                (
                                    ((INumberConstant)
                                        ((ConstantValue)
                                            ((Division)tokens[0])[0]
                                        ).GetValue()
                                    ).GetDoubleValue()
                                ) / (
                                    ((INumberConstant)
                                        ((ConstantValue)
                                            ((Division)tokens[0])[1]
                                        ).GetValue()
                                    ).GetDoubleValue()
                                )
                            )
                        )
                    })
                )
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(AbortKeyword)),
                        new Type_PatternSegment(Type_.String())
                    }, new Wrapper2PatternProcessor(
                        new SlotPatternProcessor(new List<int> {1}),
                        typeof(Abort)
                    )
                ),
            },
            new List<IMatcher> {
                new PatternMatcher(
                    new List<IPatternSegment> {
                        new TypePatternSegment(typeof(AbortKeyword))
                    }, new InstantiationPatternProcessor(
                        typeof(AbortVoid)
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

        foreach (CodeBlock block in TokenUtils.TraverseFind<CodeBlock>(program)) {
            DoBlockCodeRules(block, rules);
        }

        return program;
    }

    void DoBlockCodeRules(CodeBlock block, List<List<IMatcher>> rules) {
        for (int i = 0; i < block.Count; i++) {
            Line line = block[i] as Line;
            if (line == null) continue;
            foreach (List<IMatcher> ruleset in rules) {
                line = (Line)DoTreeCodeRules(line, ruleset);
            }
            block[i] = line;
        }
    }

    IParentToken DoTreeCodeRules(IParentToken parent_, List<IMatcher> ruleset) {
        IParentToken parent = parent_;
        bool changed = true;
        while (changed) {
            changed = false;
            for (int i = 0; i < parent.Count; i++) {
                IToken sub = parent[i];
                if (sub is IParentToken && !(sub is IBarMatchingInto || sub is CodeBlock)) {
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
        TraverseConfig config = new TraverseConfig(
            TraverseMode.DEPTH, invert: false, yieldFirst: true,
            avoidTokens: token => false
        );
        foreach (IVerifier token in TokenUtils.TraverseFind<IVerifier>(program, config)) {
            token.Verify();
        }
    }

    Dependencies GetDependencies(Program program) {
        List<RealFunctionDeclaration> declarationDependencies = new List<RealFunctionDeclaration>();
        List<Struct> structDependencies = new List<Struct>();

        HashSet<Struct> structsHere = program.GetStructsHere();
        HashSet<Function> functionsHere = new HashSet<Function>();
        foreach (IToken token in program) {
            Function function = token as Function;
            if (function != null) functionsHere.Add(function);
        }

        foreach (IValueToken token in TokenUtils.TraverseFind<IValueToken>(program)) {
            IFunctionCall call = token as IFunctionCall;
            if (call != null) {
                RealFunctionDeclaration func = call.GetFunction() as RealFunctionDeclaration;
                if (func != null && !functionsHere.Contains(func)) {
                    declarationDependencies.Add(func);
                }
            }

            Type_ type_ = token.GetType_();
            if (type_.GetBaseType_().IsBuiltin()) continue;
            Struct struct_ = StructsCtx.GetStructFromType_(type_);
            if (structsHere.Contains(struct_)) continue;
            structDependencies.Add(struct_);
        }

        return new Dependencies(structDependencies, declarationDependencies);
    }

    void AddUnusedValueWrappers(Program program) {
        foreach (CodeBlock block in TokenUtils.TraverseFind<CodeBlock>(program)) {
            for (int i = 0; i < block.Count; i++) {
                Line line = block[i] as Line;
                if (line == null) continue;
                IValueToken sub = line[0] as IValueToken;
                if (sub == null) continue;
                line[0] = new UnusedValueWrapper(sub);
            }
        }
    }

    string GetJSONString(Program program) {
        return program.GetJSON().Stringify();
    }

    void SaveJSON(string json) {
        using (StreamWriter file = new StreamWriter(Utils.JoinPaths(Utils.TempDir(), "code.json"))) {
            file.Write(json);
        }
    }

    void CreateLLVMIR() {
        CmdUtils.RunScript($"scripts{Path.DirectorySeparatorChar}runpython.bash");
    }
}

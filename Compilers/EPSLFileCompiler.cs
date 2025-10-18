using CsJSONTools;

namespace Epsilon;
public class EPSLFileCompiler : IFileCompiler {
    Program program;
    readonly string fileText;
    readonly string idPath;
    string IR;

    public static void Setup() {
        Builder.RegisterDispatcher((BuildSettings buildSettings, string path, string fileText) => {
            string idPath = buildSettings.GetIDPath(path);
            return new EPSLFileCompiler(path, idPath, fileText, buildSettings);
        }, "epsl");
    }

    EPSLFileCompiler(string realPath, string idPath, string fileText, BuildSettings buildSettings) {
        Log.Info("Compiling EPSL file", idPath);
        this.idPath = idPath;
        this.fileText = fileText;
        program = new Program(
            realPath, Utils.GetFullPath(idPath), new FileExerptManager(fileText),
            InitialTokenizer.Tokenize(fileText).ToList(), buildSettings
        );
        program.span = new CodeSpan(0, fileText.Length-1);
    }

    public string GetText() {
        return fileText;
    }

    public string GetIDPath() {
        return idPath;
    }

    public IEnumerable<string> ToImports() {
        program = TokenizeFuncSignatures(program);

        program = TokenizeNames(program);

        program = TokenizeImports(program);

        return program.Where(token => token is Import).Select(
            token => (token as Import).GetRealPath()
        );
    }

    public SubconfigCollection GetSubconfigs() {
        return SubconfigCollection.Empty();
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

        return program.GetStructIDs().Where(id => !id.IsPrivate()).ToHashSet();
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
            .Where(func => func != null && !func.IsPrivate()).ToList();
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

    public void FinishCompilation(string destPath, bool recommendLLVM) {
        AddUnusedValueWrappers(program);

        SaveProgramJSON(program);

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

    public bool FromCache() {
        return false;
    }

    public bool ShouldSaveSPEC() {
        return true;
    }

    public FileSourceType GetFileSourceType() {
        return FileSourceType.User;
    }

    Program TokenizeFuncSignatures(Program program) {
        return (Program)PerformMatching(program, new RawFuncSignatureMatcher());
    }

    Program TokenizeNames(Program program) {
        IMatcher matcher = new NameMatcher();
        foreach (IToken token in program) {
            if (token is not RawFuncSignature) continue;
            RawFuncSignature sig = (RawFuncSignature)token;
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
            if (token is not RawGlobal) continue;
            program[i] = PerformMatching((RawGlobal)token, matcher);
        }
        return program;
    }

    Program TokenizeKeywords(Program program) {
        Dictionary<string, Type> keywords = new() {
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
                [
                    new UnitsPatternSegment<string>(
                        typeof(Name), keywords.Keys.ToList()
                    )
                ], new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                    string name = ((Name)tokens[0]).GetValue();
                    return [(IToken)Activator.CreateInstance(
                        keywords[name]
                    )];
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
                if (token is TextToken text) {
                    return !Utils.Whitespace.Contains(text.GetText()[0]);
                }
                return true;
            }
        ).ToList();
    }

    Program RemoveWhitespace(Program program) {
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (token is RawFuncSignature sig) {
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
            } else if (token is RawGlobal rawGlobal) {
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
            if (token is not RawFuncSignature) continue;
            RawFuncSignature sig = (RawFuncSignature)token;
            RawFuncTemplate template = (RawFuncTemplate)sig.GetTemplate();
            sig.SetTemplate(PerformMatching(template, matcher));
        }
        return program;
    }

    Program TokenizeFunctionHolders(Program program) {
        return (Program)PerformMatching(program, new PatternMatcher(
            [
                new TypePatternSegment(typeof(RawFuncSignature)),
                new TypePatternSegment(typeof(Block))
            ], new WrapperPatternProcessor(typeof(FunctionHolder))
        ));
    }

    Program TokenizeStructHolders(Program program) {
        return (Program)PerformMatching(program, new PatternMatcher(
            [
                new TypePatternSegment(typeof(Name)),
                new TypePatternSegment(typeof(Block))
            ], new WrapperPatternProcessor(typeof(StructHolder))
        ));
    }

    Program ConvertFunctionBlocks(Program program) {
        IMatcher matcher = new PatternMatcher(
            [
                new TypePatternSegment(typeof(Block), exact: true)
            ], new FuncPatternProcessor<List<IToken>>(tokens => {
                return [new CodeBlock(
                    program, ((Block)tokens[0]).GetTokens()
                )];
            })
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (token is FunctionHolder holder) {
                program[i] = PerformTreeMatching(holder, matcher);
            }
        }
        return program;
    }

    void ComputeBaseTypes_(Program program) {
        HashSet<LocatedID> structIds = [];
        foreach (IToken token_ in program) {
            if (token_ is not StructHolder) continue;
            StructHolder token = (StructHolder)token_;
            if (token.Count == 0) continue;
            IToken nameToken = token[0];
            if (nameToken is not Name) continue;
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
            } else if (token is Holder holder) {
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
                        if (template[j] is RawFunctionArgument argument) {
                            template[j] = PerformTreeMatching(argument, matcher);
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
        Dictionary<string, Func<IConstant>> constantValues = new() {
            {"true", () => new BoolConstant(true)},
            {"false", () => new BoolConstant(false)},
            {"infinity", () => new FloatConstant(double.PositiveInfinity)},
            {"NaN", () => new FloatConstant(double.NaN)},
            {"pi", () => new FloatConstant(MathF.PI)},
        };
        return DoStructuredSyntaxMatching(
            program, new PatternMatcher(
                [
                    new UnitsPatternSegment<string>(
                        typeof(Name), constantValues.Keys.ToList()
                    )
                ], new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                    string name = ((Name)tokens[0]).GetValue();
                    return [
                        new ConstantValue(constantValues[name]())
                    ];
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
            return [new Type_Token(type_)];
        };

        return DoStructuredSyntaxMatching(
            program, new CombinedMatchersMatcher([
                new PatternMatcher(
                    [
                        new TextPatternSegment("["),
                        new TypePatternSegment(typeof(Type_Token)),
                        new TextPatternSegment("]")
                    ], new FuncPatternProcessor<List<IToken>>(
                        tokens => type_Wrapper(tokens, () =>
                            ((Type_Token)tokens[1]).GetValue().ArrayOf())
                    )
                ),
                new PatternMatcher(
                    [
                        new TextPatternSegment("$"),
                        new TypePatternSegment(typeof(Type_Token))
                    ], new FuncPatternProcessor<List<IToken>>(
                        tokens => type_Wrapper(tokens, () =>
                            ((Type_Token)tokens[1]).GetValue().PolyOf())
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(Type_Token)),
                        new TextPatternSegment("?")
                    ], new FuncPatternProcessor<List<IToken>>(
                        tokens => type_Wrapper(tokens, () =>
                            ((Type_Token)tokens[0]).GetValue().OptionalOf())
                    )
                ),
                new Type_Matcher(type_Wrapper),
            ]), true
        );
    }

    Program TokenizeVarDeclarations(Program program) {
        return DoStructuredSyntaxMatching(
            program, new PatternMatcher(
                [
                    new TypePatternSegment(typeof(Type_Token)),
                    new TextPatternSegment(":"),
                    new TypePatternSegment(typeof(Name))
                ], new Wrapper2PatternProcessor(
                    new SlotPatternProcessor([0, 2]),
                    typeof(VarDeclaration)
                )
            ), false
        );
    }

    Program ConvertTemplateArguments(Program program) {
        List<IPatternSegment> functionArgumentSegments = [
            new TypePatternSegment(typeof(Type_Token)),
            new TextPatternSegment(":"),
            new TypePatternSegment(typeof(Name))
        ];
        IMatcher argumentConverterMatcher = new PatternMatcher(
            [
                new FuncPatternSegment<RawFunctionArgument>(arg => {
                    if (!TokenUtils.FullMatch(functionArgumentSegments, arg.GetTokens())) {
                        throw new SyntaxErrorException(
                            "Malformed function argument", arg
                        );
                    }
                    return true;
                })
            ], new FuncPatternProcessor<List<IToken>>(tokens => {
                RawFunctionArgument arg = (RawFunctionArgument)tokens[0];
                return [
                    new FunctionArgumentToken(
                        ((Name)arg[2]).GetValue(), ((Type_Token)arg[0]).GetValue()
                    )
                ];
            })
        );
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (token is not FunctionHolder) continue;
            FunctionHolder holder = (FunctionHolder)token;
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
            if (token is not FunctionHolder) continue;
            FunctionHolder holder = (FunctionHolder)token;
            RawFuncSignature sig = holder.GetRawSignature();
            RawFuncTemplate rawTemplate = (RawFuncTemplate)sig.GetTemplate();
            List<IPatternSegment> segments = [];
            List<FunctionArgumentToken> arguments = [];
            List<int> slots = [];
            int j = -1;
            foreach (IToken subtoken in rawTemplate) {
                j++;
                Type tokenType = subtoken.GetType();
                IPatternSegment segment = null;
                if (subtoken is TextToken token1) {
                    segment = new TextPatternSegment(
                        token1.GetText()
                    );
                } else if (subtoken is Unit<string> unit) {
                    segment = new UnitPatternSegment<string>(
                        tokenType, unit.GetValue()
                    );
                } else if (subtoken is FunctionArgumentToken argument) {
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
            if (token is not FunctionHolder) continue;
            FunctionHolder holder = (FunctionHolder)token;
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
            [
                new TypePatternSegment(typeof(RawAnnotation)),
                new TypePatternSegment(typeof(IAnnotatable))
            ], new FuncPatternProcessor<List<IToken>>(tokens => {
                RawAnnotation rawAnnotation = (RawAnnotation)tokens[0];
                IAnnotation annotation = rawAnnotation.ToAnnotation();
                IAnnotatable annotatable = (IAnnotatable)tokens[1];
                if ((annotation.GetRecipients() & annotatable.RecipientType()) == 0) {
                    throw new SyntaxErrorException(
                        $"Cannot apply annotation of type '{rawAnnotation.AnnotationTypeName()}' to specified token", annotatable
                    );
                }
                annotatable.ApplyAnnotation(annotation);
                return [annotatable];
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
            [
                new TypePatternSegment(typeof(FunctionHolder))
            ], new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                FunctionHolder holder = (FunctionHolder)tokens[0];
                FuncSignature sig = holder.GetSignature();
                FuncTemplate template = sig.GetTemplate();
                return [
                    new Function(
                        GetIDPath(), program, template.GetValue(), template.GetArguments(),
                        (CodeBlock)holder.GetBlock(), sig.GetReturnType_(), holder.GetAnnotations()
                    )
                ];
            })
        ));
    }

    Program ComputeStructs(Program program) {
        ListTokenParser<Field> listParser = new(
            new TextPatternSegment(","), typeof(VarDeclaration),
            (token) => new Field((VarDeclaration)token)
        );
        HashSet<Struct> structs = [];
        for (int i = 0; i < program.Count; i++) {
            IToken token = program[i];
            if (token is StructHolder holder) {
                Block block = holder.GetBlock();
                if (block == null) continue;
                IToken nameT = holder[0];
                if (nameT is not Unit<string>) continue;
                Name name = (Name)nameT;
                string nameStr = name.GetValue();
                List<Field> fields = listParser.Parse(block);
                if (fields == null) {
                    throw new SyntaxErrorException(
                        "Malformed struct body", token
                    );
                }
                structs.Add(new Struct(GetIDPath(), nameStr, fields,
                    holder.GetAnnotations()));
            }
        }
        program.SetStructsHere(structs);
        return (Program)program.Copy(program.Where(token => token is not StructHolder).ToList());
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
            if (token is IParentToken parent) {
                for (int i = 0; i < parent.Count; i++) {
                    IToken sub = parent[i];
                    if (sub is CodeBlock block) {
                        parent[i] = SplitBlockIntoLines(block);
                    }
                }
            }
        }
        return program;
    }

    CodeBlock SplitBlockIntoLines(CodeBlock block) {
        SplitTokensParser parser = new(
            new TextPatternSegment(";"), false
        );
        List<List<IToken>> rawLines = parser.Parse(block);
        if (rawLines == null) {
            throw new SyntaxErrorException(
                "Missing semicolon", TokenUtils.MergeSpans(block) ?? block.span
            );
        }
        List<IToken> lines = [];
        foreach(List<IToken> section in rawLines) {
            Line line = new(section);
            line.span = TokenUtils.MergeSpans(section);
            lines.Add(line);
        }
        return (CodeBlock)block.Copy(lines);
    }

    Program ConvertStringLiterals(Program program) {
        return (Program)PerformTreeMatching(
            program, new PatternMatcher(
                [
                    new FuncPatternSegment<ConstantValue>(
                        constant => constant.GetValue() is StringConstant
                    )
                ], new FuncPatternProcessor<List<IToken>>(tokens => {
                    ConstantValue sval = (ConstantValue)tokens[0];
                    string text = ((StringConstant)sval.GetValue()).GetValue();
                    return [new StringLiteral(text)];
                })
            )
        );
    }

    Program TokenizeGroups(Program _program) {
        List<IMatcher> matchers = [
            new PatternMatcher(
                [
                    new TextPatternSegment("("),
                    new TypePatternSegment(typeof(Type_Token)),
                    new TextPatternSegment(")")
                ], new Wrapper2PatternProcessor(
                    new SlotPatternProcessor([1]),
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
        ];
        Program program = _program;
        foreach(IMatcher matcher in matchers) {
            program = (Program)PerformTreeMatching(program, matcher);
        }
        return program;
    }

    Program TokenizeForLoops(Program program) {
        return (Program)PerformTreeMatching(
            program, new SurroundedPatternMatcher(
                new TypePatternSegment(typeof(ForKeyword)),
                new TypePatternSegment(typeof(CodeBlock)),
                new FuncPatternProcessor<List<IToken>>(tokens => {
                    List<IToken> condition = tokens.Skip(1).SkipLast(1).ToList();
                    if (condition.Count == 1 && condition[0] is RawGroup group) {
                        condition = [..group];
                    }
                    if (condition.Count <= 1) {
                        throw new SyntaxErrorException(
                            "For loop condition cannot be empty and must have at least one clause", tokens[1]
                        );
                    }
                    return [new RawFor(
                        condition, (CodeBlock)tokens[^1]
                    )];
                })
            )
        );
    }

    Program TokenizeGivenBlocks(Program program) {
        return (Program)PerformTreeMatching(
            program, new SurroundedPatternMatcher(
                new TypePatternSegment(typeof(GivenKeyword)),
                new TypePatternSegment(typeof(CodeBlock)),
                new FuncPatternProcessor<List<IToken>>(tokens => {
                    List<IToken> middle = tokens.Skip(1).SkipLast(1).ToList();
                    if (middle.Count == 1 && middle[0] is RawGroup group) {
                        middle = [..group];
                    }
                    int asIdx = -1;
                    for (int i = 0; i < middle.Count; i++) {
                        IToken token = middle[i];
                        if (token is Name name && name.GetValue() == "as") {
                            asIdx = i;
                            break;
                        }
                    }
                    if (asIdx == -1) {
                        throw new SyntaxErrorException(
                            "Expected 'as' keyword", TokenUtils.MergeSpans(middle)
                        );
                    }
                    if (asIdx > middle.Count - 2) {
                        throw new SyntaxErrorException(
                            "Expected variable declaration after 'as' keyword", middle[asIdx]
                        );
                    }
                    if (asIdx < middle.Count - 2) {
                        throw new SyntaxErrorException(
                            "Expected only a variable declaration after 'as' keyword", middle[^1]
                        );
                    }
                    IToken endingToken = middle[^1];
                    if (endingToken is not VarDeclaration var_) {
                        throw new SyntaxErrorException(
                            "Expected variable declaration", endingToken
                        );
                    }
                    RawGivenValue givenValue = new(middle.Slice(asIdx));
                    CodeBlock block = (CodeBlock)tokens[^1];
                    return [new RawGivenPart(
                        givenValue, var_, block
                    )];
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
            if (token is Function function) {
                foreach (VarDeclaration declaration in TokenUtils.TraverseFind<VarDeclaration>(function)) {
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

    List<FunctionDeclaration> GetBestFunctions(List<FunctionDeclaration> matchingFunctions, List<Type_> targetTypes_) {
        for (int phase = 0; phase < 2; phase++) {
            for (int i = 0; i < targetTypes_.Count; i++) {
                if (matchingFunctions.Count == 1) goto selectedFunction;

                List<List<Type_>> functionsTypes_ = matchingFunctions
                    .Select(function =>
                        function.GetArguments()
                        .Select(argument => argument.GetType_())
                        .ToList())
                    .ToList();

                List<int> matchingIdxs = [];

                for (int j = 0; j < functionsTypes_.Count; j++) {
                    if (phase == 0) {
                        if (functionsTypes_[j][i].Equals(targetTypes_[i])) {
                            matchingIdxs.Add(j);
                        }
                    } else if (phase == 1) {
                        bool greaterThanNone = true;
                        for (int k = 0; k < functionsTypes_.Count; k++) {
                            if (functionsTypes_[j][i].IsGreaterThan(functionsTypes_[k][i])) {
                                // eight layers of indentation FTW
                                greaterThanNone = false;
                                break;
                            }
                        }
                        if (greaterThanNone) matchingIdxs.Add(j);
                    } else {
                        throw new InvalidOperationException();
                    }
                }

                if (matchingIdxs.Count > 0) {
                    List<FunctionDeclaration> newMatchingFunctions = [];
                    foreach (int idx in matchingIdxs) {
                        newMatchingFunctions.Add(matchingFunctions[idx]);
                    }
                    matchingFunctions = newMatchingFunctions;
                }
            }
        }

    selectedFunction:
        return matchingFunctions;
    }

    List<IToken> IdentifyRawFunctionCall(List<IToken> tokens) {
        RawFunctionCall rawCall = (RawFunctionCall)tokens[0];

        List<Type_> paramTypes_ = [];
        List<IValueToken> parameters = [];

        for (int i = 0; i < rawCall.Count; i++) {
            RawSquareGroup rparameter = rawCall[i] as RawSquareGroup;
            if (rparameter.Count == 0) {
                throw new SyntaxErrorException(
                    "Function parameters cannot be empty", rparameter
                );
            }
            if (rparameter.Count > 1) {
                throw new SyntaxErrorException(
                    "Illegal syntax in function parameter", rparameter
                );
            }
            if (rparameter[0] is not IValueToken parameter) {
                string message = "Function parameter must contain a value";
                if (rparameter[0] is VoidFunctionCall)
                    message += " (the function called in this parameter has no return value)";
                throw new SyntaxErrorException(message, rparameter);
            }
            paramTypes_.Add(parameter.GetType_());
            parameters.Add(parameter);
        }

        List<IEnumerable<Type_>> type_Alternatives = [];

        List<FunctionDeclaration> matchingFunctions = [];

        foreach (FunctionDeclaration function in rawCall.GetMatchingFunctions()) {
            List<FunctionArgument> args = function.GetArguments();
            if (paramTypes_.Count != args.Count) continue;

            type_Alternatives.Add(args.Select(arg => arg.GetType_()));

            bool matches = true;

            for (int i = 0; i < paramTypes_.Count; i++) {
                if (!args[i].IsCompatibleWith(paramTypes_[i])) {
                    matches = false;
                    break;
                }
            }

            if (matches) matchingFunctions.Add(function);
        }

        static string stringifyTypes_(IEnumerable<Type_> types_) =>
            string.Join(", ", types_.Select(type_ => type_.ToString()));
        string plural = paramTypes_.Count == 1 ? "" : "s";

        if (matchingFunctions.Count == 0) {
            string expectedTypes_Str;
            if (type_Alternatives.Count == 1) {
                expectedTypes_Str = stringifyTypes_(type_Alternatives[0]);
            } else {
                expectedTypes_Str = "\n" + string.Join(
                    " or\n", type_Alternatives.Select(stringifyTypes_));
            }

            throw new SyntaxErrorException(
                $@"Types supplied to function do not match any overload:
Got type{plural}: {stringifyTypes_(paramTypes_)}
Expected type{plural}: {expectedTypes_Str}", rawCall
            );
        } else {
            List<FunctionDeclaration> functions = GetBestFunctions(matchingFunctions, paramTypes_);

            if (functions.Count == 1) {
                FunctionDeclaration function = functions[0];
                function.VerifyPassedTokens(parameters); // verify that the passed parameters are of the correct types_
                IFunctionCall call;
                if (function.DoesReturnVoid()) {
                    call = new VoidFunctionCall(function, parameters);
                } else {
                    call = new FunctionCall(function, parameters);
                }
                return [call];
            } else {
                string bestFuncTypes_Str = string.Join(" or\n", functions.Select(
                    func => stringifyTypes_(func.GetArguments().Select(arg => arg.GetType_()))));
                throw new SyntaxErrorException($@"Function call is ambiguous:
Got type{plural}: {stringifyTypes_(paramTypes_)}
Please clarify between the functions that take the types:
{bestFuncTypes_Str}", rawCall);
            }
        }
    }

    readonly Dictionary<string, Type> FloatCompoundableOperators = new() {
        {"+", typeof(Addition)}, {"-", typeof(Subtraction)},
        {"*", typeof(Multiplication)}, {"/", typeof(Division)},
        {"%", typeof(Modulo)}
    };

    List<IToken> TransformCompoundAssignments(List<IToken> tokens) {
        IAssignableValue assignable = (IAssignableValue)tokens[0];
        IValueToken value = (IValueToken)tokens[3];

        IValueToken newValue = (IValueToken)Activator.CreateInstance(
            FloatCompoundableOperators[
                ((TextToken)tokens[1]).GetText()
            ], assignable, value
        );
        newValue.span = TokenUtils.MergeSpans(tokens);

        IAssignment assignment = assignable.AssignTo(newValue);
        newValue.span = newValue.span;
        return [assignment];
    }

    List<IToken> TransformInplaceOffset(IToken operand, bool increment, bool pre) {
        IAssignableValue assignable = (IAssignableValue)operand;

        IValueToken newValue;
        if (increment) {
            newValue = new AddOne(assignable);
        } else {
            newValue = new SubOne(assignable);
        }
        newValue.span = operand.span;

        IAssignment assignment = assignable.AssignTo(newValue);
        assignment.span = operand.span;

        IValueToken result;
        if (pre) {
            result = newValue;
        } else {
            result = assignable;
        }

        CommaOperation comma = new(assignment, result);
        comma.span = operand.span;
        return [comma];
    }

    Program ParseFunctionCode(Program program) {
        List<IMatcher> functionRules = [];
        List<IMatcher> addMatchingFunctionRules = [];

        List<FunctionDeclaration> functions = [..BuiltinsList.Builtins, ..program.GetExternalDeclarations()];

        functions.AddRange(program.OfType<Function>());

        functions.Sort(FunctionShapeComparer.Singleton);

        List<PatternExtractor<List<IToken>>> extractors = [];
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

        List<List<IMatcher>> rules = [
            functionRules,
            addMatchingFunctionRules,
            [
                new PatternMatcher(
                    [
                        new TextPatternSegment("."),
                        new TypePatternSegment(typeof(Name))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([1]),
                        typeof(MemberAccessPostfix)
                    )
                )
            ],
            [
                new PatternMatcher(
                    [
                        new FuncPatternSegment<Name>(
                            (Name name) => Scope.ContainsVar(name, name.GetValue())
                        ),
                    ], new Wrapper2PatternProcessor(
                        typeof(Variable)
                    )
                )
            ],
            [
                new GroupConverterMatcher(typeof(RawGroup), typeof(Group)),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(RawFunctionCall))
                    ], new FuncPatternProcessor<List<IToken>>(IdentifyRawFunctionCall)
                ),
                new PatternMatcher(
                    [
                        new FuncPatternSegment<RawSquareGroup>(
                            (RawSquareGroup group) => group.parent is not RawFunctionCall
                        )
                    ], new WrapperPatternProcessor(
                        new SplitTokensPatternProcessor(
                            new UnwrapperPatternProcessor(),
                            new TextPatternSegment(","),
                            typeof(ValueListItem)
                        ),
                        typeof(ValueList)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(Type_Token)),
                        new TypePatternSegment(typeof(ValueList))
                    ], new Wrapper2PatternProcessor(
                        typeof(Instantiation)
                    )
                ),
                new PatternMatcher(
                    [
                        new FuncPatternSegment<Instantiation>(
                            i => i.GetType_().GetBaseType_().GetName() == "Array"
                        )
                    ], new Wrapper2PatternProcessor(
                        typeof(ArrayCreation)
                    )
                ),
                new PatternMatcher(
                    [
                        new Type_PatternSegment(Type_.Any().ArrayOf()),
                        new TypePatternSegment(typeof(ValueList)),
                        new TextPatternSegment("?")
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0, 1]),
                        typeof(OptionalArrayAccess)
                    )
                ),
                new PatternMatcher(
                    [
                        new Type_PatternSegment(Type_.Any().ArrayOf()),
                        new TypePatternSegment(typeof(ValueList))
                    ], new Wrapper2PatternProcessor(
                        typeof(ArrayAccess)
                    )
                ),
                new PatternMatcher(
                    [
                        new Type_PatternSegment(Type_.Any()),
                        new TypePatternSegment(typeof(MemberAccessPostfix))
                    ], new Wrapper2PatternProcessor(
                        typeof(MemberAccess)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(BreakKeyword))
                    ], new InstantiationPatternProcessor(typeof(Break))
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(ContinueKeyword))
                    ], new InstantiationPatternProcessor(typeof(Continue))
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(Given)),
                        new TypePatternSegment(typeof(RawGivenPart))
                    ], new Wrapper2PatternProcessor(
                        typeof(Given)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(RawGivenPart))
                    ], new Wrapper2PatternProcessor(
                        typeof(Given)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(Given)),
                        new TypePatternSegment(typeof(ElseKeyword)),
                        new TypePatternSegment(typeof(CodeBlock))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0, 2]),
                        typeof(Given)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(IfKeyword)),
                        new TypePatternSegment(typeof(IValueToken)),
                        new TypePatternSegment(typeof(CodeBlock))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([1, 2]),
                        typeof(Conditional)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(Conditional)),
                        new TypePatternSegment(typeof(ElseIfKeyword)),
                        new TypePatternSegment(typeof(IValueToken)),
                        new TypePatternSegment(typeof(CodeBlock))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0, 2, 3]),
                        typeof(Conditional)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(Conditional)),
                        new TypePatternSegment(typeof(ElseKeyword)),
                        new TypePatternSegment(typeof(CodeBlock))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0, 2]),
                        typeof(Conditional)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(RawFor))
                    ], new Wrapper2PatternProcessor(
                        typeof(For)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(WhileKeyword)),
                        new TypePatternSegment(typeof(IValueToken)),
                        new TypePatternSegment(typeof(CodeBlock))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([1, 2]),
                        typeof(While)
                    )
                ),
                new AdvancedPatternMatcher(
                    [
                        new TypePatternSegment(typeof(SwitchKeyword)),
                        new TypePatternSegment(typeof(IValueToken))
                    ], [
                        new TypePatternSegment(typeof(Group)),
                        new TypePatternSegment(typeof(CodeBlock))
                    ], 1, -1, [
                        new TypePatternSegment(typeof(CodeBlock))
                    ],
                    new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                        return [
                            new Switch((IValueToken)tokens[1], tokens.Skip(2).ToArray())
                        ];
                    })
                ),
                new AdvancedPatternMatcher(
                    [
                        new TypePatternSegment(typeof(SwitchKeyword)),
                        new TypePatternSegment(typeof(IValueToken))
                    ], [
                        new TypePatternSegment(typeof(Group)),
                        new TypePatternSegment(typeof(CodeBlock))
                    ], 1, -1, [],
                    new FuncPatternProcessor<List<IToken>>((List<IToken> tokens) => {
                        return [
                            new Switch((IValueToken)tokens[1], tokens.Skip(2).ToArray())
                        ];
                    })
                ),
                new PatternMatcher(
                    [
                        new TextPatternSegment("!"),
                        new Type_PatternSegment(Type_.Any())
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([1]),
                        typeof(Not)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(Type_Token)),
                        new TextPatternSegment("*"),
                        new Type_PatternSegment(new Type_("W"))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0, 2]),
                        typeof(ZeroedArrayCreation)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(UnmatchedCast)),
                        new Type_PatternSegment(Type_.Any())
                    ], new Wrapper2PatternProcessor(typeof(Cast))
                ),
                new CombinedMatchersMatcher([
                    new PatternMatcher(
                        [
                            new AndPatternSegment(
                                new Type_PatternSegment(new Type_("R")),
                                new TypePatternSegment(typeof(IAssignableValue))
                            ),
                            new TextPatternSegment("+"),
                            new TextPatternSegment("+")
                        ], new FuncPatternProcessor<List<IToken>>(tokens => TransformInplaceOffset(
                            tokens[0], increment: true, pre: false
                        ))
                    ),
                    new PatternMatcher(
                        [
                            new AndPatternSegment(
                                new Type_PatternSegment(new Type_("R")),
                                new TypePatternSegment(typeof(IAssignableValue))
                            ),
                            new TextPatternSegment("-"),
                            new TextPatternSegment("-")
                        ], new FuncPatternProcessor<List<IToken>>(tokens => TransformInplaceOffset(
                            tokens[0], increment: false, pre: false
                        ))
                    ),
                    new PatternMatcher(
                        [
                            new TextPatternSegment("+"),
                            new TextPatternSegment("+"),
                            new AndPatternSegment(
                                new Type_PatternSegment(new Type_("R")),
                                new TypePatternSegment(typeof(IAssignableValue))
                            )
                        ], new FuncPatternProcessor<List<IToken>>(tokens => TransformInplaceOffset(
                            tokens[2], increment: true, pre: true
                        ))
                    ),
                    new PatternMatcher(
                        [
                            new TextPatternSegment("-"),
                            new TextPatternSegment("-"),
                            new AndPatternSegment(
                                new Type_PatternSegment(new Type_("R")),
                                new TypePatternSegment(typeof(IAssignableValue))
                            )
                        ], new FuncPatternProcessor<List<IToken>>(tokens => TransformInplaceOffset(
                            tokens[2], increment: false, pre: true
                        ))
                    )
                ]),
                new PatternMatcher(
                    [
                        new Type_PatternSegment(new Type_("R")),
                        new TextPatternSegment("*"),
                        new TextPatternSegment("*"),
                        new Type_PatternSegment(new Type_("R"))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0, 3]),
                        typeof(Exponentiation)
                    )
                ),
                new PatternMatcher(
                    [
                        new TextPatternSegment("-"),
                        new Type_PatternSegment(new Type_("R"))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([1]),
                        typeof(Negation)
                    )
                ),
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(FormatChain)),
                        new TextPatternSegment("%"),
                        new Type_PatternSegment(Type_.Any())
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0, 2]),
                        typeof(FormatChain)
                    )
                ),
                new PatternMatcher(
                    [
                        new Type_PatternSegment(Type_.String()),
                        new TextPatternSegment("%"),
                        new Type_PatternSegment(Type_.Any())
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0, 2]),
                        typeof(FormatChain)
                    )
                ),
                new CombinedMatchersMatcher([
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TextPatternSegment("*"),
                            new Type_PatternSegment(new Type_("R"))
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 2]),
                            typeof(Multiplication)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TextPatternSegment("/"),
                            new Type_PatternSegment(new Type_("R"))
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 2]),
                            typeof(Division)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("Z")),
                            new TextPatternSegment("~"),
                            new TextPatternSegment("/"),
                            new Type_PatternSegment(new Type_("Z"))
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 3]),
                            typeof(IntDivision)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TextPatternSegment("%"),
                            new Type_PatternSegment(new Type_("R"))
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 2]),
                            typeof(Modulo)
                        )
                    ),
                ]),
                new CombinedMatchersMatcher([
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TextPatternSegment("+"),
                            new Type_PatternSegment(new Type_("R"))
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 2]),
                            typeof(Addition)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TextPatternSegment("-"),
                            new Type_PatternSegment(new Type_("R"))
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 2]),
                            typeof(Subtraction)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TypePatternSegment(typeof(Negation))
                        ], new FuncPatternProcessor<List<IToken>>(tokens => [
                            new Subtraction(
                                (IValueToken)tokens[0], ((Negation)tokens[1]).Sub()
                            )
                        ])
                    ),
                ]),
                new CombinedMatchersMatcher([
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TextPatternSegment(">"),
                            new Type_PatternSegment(new Type_("R"))
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 2]),
                            typeof(Greater)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TextPatternSegment("<"),
                            new Type_PatternSegment(new Type_("R"))
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 2]),
                            typeof(Less)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TextPatternSegment(">"),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(new Type_("R"))
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 3]),
                            typeof(GreaterEqual)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(new Type_("R")),
                            new TextPatternSegment("<"),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(new Type_("R")),
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 3]),
                            typeof(LessEqual)
                        )
                    ),
                ]),
                new CombinedMatchersMatcher([
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("="),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(Type_.Any())
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 3]),
                            typeof(Equals)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("!"),
                            new TextPatternSegment("="),
                            new Type_PatternSegment(Type_.Any())
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 3]),
                            typeof(NotEquals)
                        )
                    ),
                ]),
                new CombinedMatchersMatcher([
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("&"),
                            new TextPatternSegment("&"),
                            new Type_PatternSegment(Type_.Any())
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 3]),
                            typeof(And)
                        )
                    ),
                    new PatternMatcher(
                        [
                            new Type_PatternSegment(Type_.Any()),
                            new TextPatternSegment("|"),
                            new TextPatternSegment("|"),
                            new Type_PatternSegment(Type_.Any())
                        ], new Wrapper2PatternProcessor(
                            new SlotPatternProcessor([0, 3]),
                            typeof(Or)
                        )
                    ),
                ]),
            ],
            [
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(VarDeclaration)),
                        new TextPatternSegment("="),
                        new Type_PatternSegment(Type_.Any())
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0, 2]),
                        typeof(InitialAssignment)
                    )
                )
            ],
            [
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(VarDeclaration))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([0]),
                        typeof(UninitVarDeclaration)
                    )
                )
            ],
            [
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(IAssignableValue)),
                        new TextPatternSegment("="),
                        new Type_PatternSegment(Type_.Any())
                    ], new FuncPatternProcessor<List<IToken>>(tokens => [
                        ((IAssignableValue)tokens[0]).AssignTo(
                            (IValueToken)tokens[2]
                        )
                    ])
                )
            ],
            [
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(IAssignableValue)),
                        new TextsPatternSegment(
                            [..FloatCompoundableOperators.Keys]
                        ),
                        new TextPatternSegment("="),
                        new Type_PatternSegment(new Type_("R"))
                    ], new FuncPatternProcessor<List<IToken>>(TransformCompoundAssignments)
                )
            ],
            [new PatternMatcher(
                [
                    new FuncPatternSegment<Division>(division =>
                        (division[0] is ConstantValue v1)
                        && (division[1] is ConstantValue v2)
                        && (v1.GetValue() is INumberConstant)
                        && (v2.GetValue() is INumberConstant)
                    )
                ], new FuncPatternProcessor<List<IToken>>(tokens => {
                    Division division = (Division)tokens[0];
                    INumberConstant lhs = (INumberConstant)((ConstantValue)division[0]).GetValue();
                    INumberConstant rhs = (INumberConstant)((ConstantValue)division[2]).GetValue();
                    return [new ConstantValue(new FloatConstant(
                        lhs.GetDoubleValue() / rhs.GetDoubleValue()
                    ))];
                })
            )],
            [
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(AbortKeyword)),
                        new TypePatternSegment(typeof(ValueList))
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([1]),
                        typeof(Abort)
                    )
                ),
            ],
            [
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(AbortKeyword))
                    ], new InstantiationPatternProcessor(
                        typeof(AbortVoid)
                    )
                ),
            ],
            [
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(ReturnKeyword)),
                        new Type_PatternSegment(Type_.Any())
                    ], new Wrapper2PatternProcessor(
                        new SlotPatternProcessor([1]),
                        typeof(Return)
                    )
                ),
            ],
            [
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(ReturnKeyword))
                    ], new InstantiationPatternProcessor(
                        typeof(ReturnVoid)
                    )
                ),
            ],
            [
                new PatternMatcher(
                    [
                        new TypePatternSegment(typeof(Group)),
                    ], new UnwrapperPatternProcessor()
                ),
            ],
        ];

        foreach (CodeBlock block in TokenUtils.TraverseFind<CodeBlock>(program)) {
            DoBlockCodeRules(block, rules);
        }

        return program;
    }

    void DoBlockCodeRules(CodeBlock block, List<List<IMatcher>> rules) {
        for (int i = 0; i < block.Count; i++) {
            if (block[i] is not Line line) continue;
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
                if (sub is IParentToken subparent && !(sub is IBarMatchingInto || sub is CodeBlock)) {
                    parent[i] = DoTreeCodeRules(subparent, ruleset);
                }
            }
            if (parent is TreeToken tree) {
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
                if (sub is IParentToken token && sub is not IBarMatchingInto) {
                    IParentToken subparent = token;
                    bool tchanged;
                    (tchanged, parent[i]) = PerformTreeMatching_(subparent, matcher);
                    changed |= tchanged;
                }
            }

            if (parent is TreeToken tree) {
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
            tree = match.Replace(tree);
        }
        return tree;
    }

    (bool, TreeToken) PerformMatchingChanged(TreeToken tree, IMatcher matcher) {
        bool changed = false;
        while (true) {
            Match match = matcher.Match(tree);
            if (match == null) break;
            changed = true;
            tree = match.Replace(tree);
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
        TraverseConfig config = new(
            TraverseMode.DEPTH, invert: false, yieldFirst: true,
            avoidTokens: token => false
        );
        foreach (IVerifier token in TokenUtils.TraverseFind<IVerifier>(program, config)) {
            token.Verify();
        }
    }

    Dependencies GetDependencies(Program program) {
        List<RealFunctionDeclaration> declarationDependencies = [];
        List<Struct> structDependencies = [];

        HashSet<Struct> structsHere = program.GetStructsHere();
        HashSet<Function> functionsHere = program.OfType<Function>().ToHashSet();

        foreach (IValueToken token in TokenUtils.TraverseFind<IValueToken>(program)) {
            if (token is IFunctionCall call) {
                if (call.GetFunction() is RealFunctionDeclaration func
                    && !functionsHere.Contains(func)) {
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
                if (block[i] is not Line line) continue;
                if (line[0] is not IValueToken sub) continue;
                line[0] = new UnusedValueWrapper(sub);
            }
        }
    }

    void SaveProgramJSON(Program program) {
        IJSONValue json = program.GetJSON();
        BinJSONEnv.WriteFile(Utils.JoinPaths(Utils.TempDir(), "code.binjson"), json);
    }

    void CreateLLVMIR() {
        CmdUtils.RunProjExecutable($"LLVMIRBuilder{Path.DirectorySeparatorChar}result");
    }
}

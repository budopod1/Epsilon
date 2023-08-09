using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
public class ListTokenParser<T> {
    enum ParseState {
        ExpectItem,
        ExpectSeperator
    }
    
    Type seperator;
    Type item;
    Func<IToken, T> parser;
    
    public ListTokenParser(Type seperator, Type item, Func<IToken, T> parser) {
        this.seperator = seperator;
        this.item = item;
        this.parser = parser;
    }
    public List<T> Parse(TreeToken tree) {
        List<T> list = new List<T>();
        ParseState state = ParseState.ExpectItem;
        foreach (IToken token in tree) {
            switch (state) {
                case ParseState.ExpectItem:
                    if (Utils.IsInstance(token, item)) {
                        list.Add(parser(token));
                        state = ParseState.ExpectSeperator;
                    } else {
                        return null;
                    }
                    break;
                case ParseState.ExpectSeperator:
                    if (Utils.IsInstance(token, seperator)) {
                        state = ParseState.ExpectItem;
                    } else {
                        return null;
                    }
                    break;
            }
        }
        return list;
    }
}
public class Utils {
    public static string Tab = "    ";
    public static string Numbers = "1234567890";
    public static string Uppercase = "QWERTYUIOPASDFGHJKLZXCVBNM";
    public static string Lowercase = "qwertyuiopasdfghjklzxcvbnm";
    public static string NameStartChars = Uppercase + Lowercase + "_";
    public static string NameChars = Uppercase + Lowercase + Numbers + "_";
    
    public static string WrapName(string name, string content, 
                                  string wrapStart="(", string wrapEnd=")") {
        return name + wrapStart + content + wrapEnd;
    }
    public static string WrapNewline(string text) {
        return "\n" + text + "\n";
    }
    public static string Indent(string text) {
        return Utils.Tab + text.Replace("\n", "\n" + Utils.Tab);
    }
    public static bool IsInstance(Type a, Type b) {
        if (a.IsGenericType)
            a = a.GetGenericTypeDefinition();
        if (b.IsGenericType)
            b = b.GetGenericTypeDefinition();
        if (a.IsSubclassOf(b)) return true;
        return a == b;
    }
    
    public static bool IsInstance(Object a, Type b) {
        return Utils.IsInstance(a.GetType(), b);
    }
}
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
        
        Console.WriteLine("Compiling structs...");
        program = CompileStructs(program);
        Console.WriteLine("Tokenize template features...");
        program = TokenizeTemplateFeatures(program);
        
        Console.WriteLine("Parsing template features...");
        program = ParseFunctionTemplates(program);
        
        Console.WriteLine(program.ToString());
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
                    segment = new Type_PatternSegment(
                        argument.GetType_()
                    );
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
        IMatcher symbolMatcher = new SymbolMatcher(
            new Dictionary<string, Type> {
                {"<", typeof(GenericsOpen)},
                {">", typeof(GenericsClose)}
            }
        );
        IMatcher nameMatcher = new NameMatcher();
        IMatcher baseMatcher = new UnitSwitcherMatcher<string>(
            typeof(Name), program.GetBaseTypes_(), typeof(BaseTokenType_)
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
    Program TokenizeBaseTypes_(Program program) {
        program = (Program)program.Copy();
        foreach (IToken token in program) {
            if (token is Holder) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                TreeToken result = PerformTreeMatching(block, 
                    new UnitSwitcherMatcher<string>(
                        typeof(Name), program.GetBaseTypes_(), typeof(BaseTokenType_)
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
    void ComputeBaseTypes_(Program program) {
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
        program.SetBaseTypes_(types_);
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
public class ArgumentConverterMatcher : IMatcher {
    Type oldArgument;
    Type nameType;
    Type type_TokenType;
    Type newArgument;
    
    public ArgumentConverterMatcher(Type oldArgument, Type name, Type type_Token,
                                    Type newArgument) {
        this.oldArgument = oldArgument;
        nameType = name;
        type_TokenType = type_Token;
        this.newArgument = newArgument;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (Utils.IsInstance(token, oldArgument)) {
                Unit<string> name = null;
                Unit<Type_> type_Token = null;
                foreach (IToken subtoken in (TreeToken)token) {
                    if (Utils.IsInstance(subtoken, nameType)) {
                        name = ((Unit<string>)subtoken);
                    } else if (Utils.IsInstance(subtoken, type_TokenType)) {
                        type_Token = ((Unit<Type_>)subtoken);
                    }
                }
                if (name == null || type_Token == null) {
                    throw new InvalidOperationException(
                        "RawFunctionArgument is incomplete"
                    );
                }
                IToken replacement = (IToken)Activator.CreateInstance(
                    newArgument, new object[] {
                        name.GetValue(), type_Token.GetValue()
                    }
                );
                return new Match(i, i, new List<IToken> {replacement},
                                 new List<IToken> {token});
            }
        }
        return null;
    }
}
public class BlockMatcher : IMatcher {
    Type start;
    Type end;
    Type holder;
    
    public BlockMatcher(Type start, Type end, Type holder) {
        this.start = start;
        this.end = end;
        this.holder = holder;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            int indent = 0;
            bool any = false;
            List<IToken> replaced = new List<IToken>();
            for (int j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                replaced.Add(token);
                if (Utils.IsInstance(token, start)) {
                    indent++;
                } else if (Utils.IsInstance(token, end)) {
                    if (!any) {
                        break;
                    }
                    indent--;
                }
                if (indent == 0) {
                    if (any) {
                        List<IToken> replacement = new List<IToken>();
                        List<IToken> replace = new List<IToken>(replaced);
                        replace.RemoveAt(0);
                        replace.RemoveAt(replace.Count-1);
                        IToken holderToken = (IToken)Activator.CreateInstance(holder, new object[] {replace});
                        replacement.Add(holderToken);
                        return new Match(i, j, replacement, replaced);
                    } else {
                        break;
                    }
                }
                any = true;
            }
        }
        return null;
    }
}
public class FloatMatcher : IMatcher {
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            List<IToken> replaced = new List<IToken>();
            bool dot = false;
            bool anyMatch = false;
            int j;
            for (j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (!(token is TextToken)) {
                    break;
                }
                string digit = ((TextToken)token).GetText();
                bool foundMatch = false;
                if (digit == "-" && !anyMatch) {
                    foundMatch = true;
                } else if ("1234567890".Contains(digit)) {
                    foundMatch = true;
                } else if (digit == "." && !dot) {
                    dot = true;
                    foundMatch = true;
                }
                anyMatch |= foundMatch;
                if (!foundMatch) {
                    break;
                }
                replaced.Add(token);
            }
            if (anyMatch && dot) {
                return new Match(i, j-1, replaced, new List<IToken>());
            }
        }
        return null;
    }
}
public class FunctionArgumentMatcher : IMatcher {
    string start;
    string end;
    Type holderType;
    
    public FunctionArgumentMatcher(string start, string end, Type holder) {
        this.start = start;
        this.end = end;
        holderType = holder;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            bool first = true;
            int indentCount = 0;
            List<IToken> replaced = new List<IToken>();
            List<IToken> replacementTokens = new List<IToken>();
            for (int j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (!(token is TextToken)) break;
                TextToken ttoken = ((TextToken)token);
                string text = ttoken.GetText();
                if ((text != start) && first) break;
                if (text == start) indentCount++;
                if (text == end) indentCount--;
                if (indentCount == 0) {
                    IToken holder = (IToken)Activator.CreateInstance(
                        holderType, new object[] {replacementTokens}
                    );
                    return new Match(i, j, new List<IToken> {holder}, replaced);
                }
                if (!first) replacementTokens.Add(token);
                first = false;
            }
        }
        return null;
    }
}
public class FunctionHolderMatcher : IMatcher {
    Type templateType;
    Type blockType;
    Type holderType;
    
    public FunctionHolderMatcher(Type template, Type block, Type holder) {
        templateType = template;
        blockType = block;
        holderType = holder;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            IToken a = tokens[i];
            IToken b = tokens[i+1];
            if (Utils.IsInstance(a, templateType) && Utils.IsInstance(b, blockType)) {
                List<IToken> replaced = new List<IToken> {a, b};
                IToken holder = (IToken)Activator.CreateInstance(
                    holderType, new object[] {replaced}
                );
                return new Match(i, i+1, new List<IToken> {holder}, replaced);
            }
        }
        return null;
    }
}
public interface IMatcher {
    Match Match(TreeToken token);
}
public class IntMatcher : IMatcher {
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            List<IToken> replaced = new List<IToken>();
            bool anyMatch = false;
            int j;
            for (j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (!(token is TextToken)) {
                    break;
                }
                bool foundMatch = false;
                string digit = ((TextToken)token).GetText();
                if (digit == "-" && !anyMatch) {
                    foundMatch = true;
                } else if ("1234567890".Contains(digit)) {
                    foundMatch = true;
                }
                anyMatch |= foundMatch;
                if (!foundMatch) {
                    break;
                }
                replaced.Add(token);
            }
            if (anyMatch) {
                return new Match(i, j-1, replaced, new List<IToken>());
            }
        }
        return null;
    }
}
public class Match {
    int start;
    int end;
    List<IToken> replacement;
    List<IToken> matched;
    
    public Match(int start, int end, List<IToken> replacement,
                 List<IToken> matched) {
        this.start = start;
        this.end = end;
        this.replacement = replacement;
        this.matched = matched;
        // Console.WriteLine(this.ToString());
    }
    public TreeToken Replace(TreeToken tokens) {
        List<IToken> result = new List<IToken>();
        int i = 0;
        foreach (IToken token in tokens) {
            if (i > this.end || i < this.start) {
                result.Add(token);
            } else if (i == this.start) {
                result.AddRange(this.replacement);
            }
            i++;
        }
        return tokens.Copy(result);
    }
    public void SetReplacement(List<IToken> replacement) {
        this.replacement = replacement;
    }
    public List<IToken> GetMatched() {
        return this.matched;
    }
    public override string ToString() {
        string result = "(" + this.start.ToString();
        result += ", " + this.end.ToString() + "):";
        foreach (IToken token in this.matched) {
            result += token.ToString();
        }
        result += "|";
        foreach (IToken token in this.replacement) {
            result += token.ToString();
        }
        return Utils.WrapName(this.GetType().Name, result);
    }
}
public class NameMatcher : IMatcher {
    public Match Match(TreeToken tokens) {
        int i = -1;
        foreach (IToken stoken in tokens) {
            i += 1;
            if (!(stoken is TextToken)) {
                continue;
            }
            string name = ((TextToken)stoken).GetText();
            if (!Utils.NameStartChars.Contains(name)) {
                continue;
            }
            List<IToken> replaced = new List<IToken>();
            replaced.Add(stoken);
            int j;
            for (j = i+1; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (token is TextToken) {
                    string text = ((TextToken)token).GetText();
                    
                    if (Utils.NameChars.Contains(text)) {
                        replaced.Add(token);
                        name += text;
                        continue;
                    }
                }
                break;
            }
            List<IToken> replacement = new List<IToken>();
            replacement.Add(new Name(name));
            return new Match(i, j-1, replacement, replaced);
        }
        return null;
    }
}
public class RawFuncTemplateMatcher : IMatcher {
    char startChar;
    char endMarkerChar;
    Type holderType;
    
    public RawFuncTemplateMatcher(char start, char endMarker, Type holder) {
        startChar = start;
        endMarkerChar = endMarker;
        holderType = holder;
    }
    
    public Match Match(TreeToken tokens) {
        int i = -1;
        foreach (IToken stoken in tokens) {
            i += 1;
            if (!(stoken is TextToken)) {
                continue;
            }
            if (((TextToken)stoken).GetText() != startChar.ToString()) {
                continue;
            }
            List<IToken> replaced = new List<IToken>();
            List<IToken> replacementTokens = new List<IToken>();
            replaced.Add(stoken);
            int j;
            for (j = i+1; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (token is TextToken) {
                    string text = ((TextToken)token).GetText();
                    
                    if (text != endMarkerChar.ToString()) {
                        replaced.Add(token);
                        if (text.Trim().Length > 0) {
                            replacementTokens.Add(token);
                        }
                        continue;
                    }
                }
                break;
            }
            List<IToken> replacement = new List<IToken>();
            // replace with reflection?
            replacement.Add(new RawFuncTemplate(replacementTokens)); 
            return new Match(i, j-1, replacement, replaced);
        }
        return null;
    }
}
public class StringMatcher : IMatcher {
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (!(token is TextToken) || ((TextToken)token).GetText() != "\"") {
                continue;
            }
            
            List<IToken> matched = new List<IToken>();
            matched.Add(token);
            bool wasBackslash = false;
            for (int j = i + 1; j < tokens.Count; j++) {
                token = tokens[j];
                if (!(token is TextToken)) {
                    continue;
                }
                matched.Add(token);
                string text = ((TextToken)token).GetText();
                if (wasBackslash) {
                    wasBackslash = false;
                } else {
                    if (text == "\\") {
                        wasBackslash = true;
                    } else if (text == "\"") {
                        return new Match(i, j, new List<IToken>(),
                                         matched);
                    }
                }
            }
            
        }
        return null;
    }
}
public class StructCompilerMatcher : IMatcher {
    Type structHolderType;
    Type structCompiledType;
    ListTokenParser<Field> listParser;
    
    public StructCompilerMatcher(Type structHolderType, Type structCompiledType,
                               ListTokenParser<Field> listParser) {
        this.structHolderType = structHolderType;
        this.structCompiledType = structCompiledType;
        this.listParser = listParser;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (Utils.IsInstance(token, structHolderType)) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                IToken nameT = holder[0];
                if (!(nameT is Unit<string>)) continue;
                Name name = ((Name)nameT);
                string nameStr = name.GetValue();
                List<Field> fields = listParser.Parse(block);
                IToken compiled = (IToken)Activator.CreateInstance(
                    structCompiledType, new object[] {
                        nameStr, fields
                    }
                );
                List<IToken> replaced = new List<IToken> {token};
                return new Match(i, i, new List<IToken> {compiled}, replaced);
            }
        }
        return null;
    }
}
public class StructHolderMatcher : IMatcher {
    Type nameType;
    Type blockType;
    Type holderType;
    
    public StructHolderMatcher(Type name, Type block, Type holder) {
        nameType = name;
        blockType = block;
        holderType = holder;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            IToken a = tokens[i];
            IToken b = tokens[i+1];
            if (Utils.IsInstance(a, nameType) && Utils.IsInstance(b, blockType)) {
                List<IToken> replaced = new List<IToken> {a, b};
                IToken holder = (IToken)Activator.CreateInstance(
                    holderType, new object[] {replaced}
                );
                return new Match(i, i+1, new List<IToken> {holder}, replaced);
            }
        }
        return null;
    }
}
public class SymbolMatcher : IMatcher {
    Dictionary<string, Type> symbols;
    public SymbolMatcher(Dictionary<string, Type> symbols) {
        this.symbols = symbols;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            List<string> possibleSymbols = new List<string>(this.symbols.Keys);
            List<IToken> replaced = new List<IToken>();
            int k = -1;
            for (int j = i; j < tokens.Count; j++) {
                k++;
                IToken token = tokens[j];
                if (!(token is TextToken)) break;
                replaced.Add(token);
                char chr = ((TextToken)token).GetText()[0];
                for (int l = 0; l < possibleSymbols.Count; l++) {
                    string symbol = possibleSymbols[l];
                    if (symbol[k] != chr) {
                        possibleSymbols.RemoveAt(l);
                        l--; // account for a removal shifting later
                        // items down by one
                        continue;
                    }
                    if (k == symbol.Length-1) {
                        List<IToken> replacement = new List<IToken>();
                        Type type = this.symbols[symbol];
                        if (type != null) {
                            Object result = Activator.CreateInstance(this.symbols[symbol]);
                            replacement.Add((IToken)result);
                        }
                        return new Match(i, j, replacement, replaced);
                    }
                }
                if (possibleSymbols.Count == 0) {
                    break;
                }
            }
        }
        return null;
    }
}
public class Type_Matcher : IMatcher {
    Type baseType;
    Type genericsType;
    Type replaceType;
    ListTokenParser<Type_> listParser;
    
    public Type_Matcher(Type baseType, Type genericsType, Type replaceType, 
                        ListTokenParser<Type_> listParser) {
        this.baseType = baseType;
        this.genericsType = genericsType;
        this.replaceType = replaceType;
        this.listParser = listParser;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken name = tokens[i];
            
            if (Utils.IsInstance(name, baseType)) {
                Unit<string> nameUnit = ((Unit<string>)name);
                Type_ type_;
                List<IToken> replacement;
                List<IToken> replaced;
                if (i + 1 < tokens.Count) {
                    IToken next = tokens[i + 1];
                    if (Utils.IsInstance(next, genericsType)) {
                        TreeToken generics = ((TreeToken)next);
                        List<Type_> genericTypes_ = listParser.Parse(generics);
                        if (genericTypes_ == null) continue;
                        type_ = new Type_(nameUnit.GetValue(), genericTypes_);
                        replacement = new List<IToken> {
                            (IToken)Activator.CreateInstance(
                                replaceType, new object[] {type_}
                            )
                        };
                        replaced = new List<IToken> {
                            name, generics
                        };
                        return new Match(i, i+1, replacement, replaced);
                    }
                }
                type_ = new Type_(nameUnit.GetValue());
                replacement = new List<IToken> {
                    (IToken)Activator.CreateInstance(
                        replaceType, new object[] {type_}
                    )
                };
                replaced = new List<IToken> {name};
                return new Match(i, i, replacement, replaced);
            }
        }
        return null;
    }
}
public class UnitSwitcherMatcher<T> : IMatcher where T : IEquatable<T> {
    Type matchType;
    List<T> matchValues;
    Type replaceType;
    
    public UnitSwitcherMatcher(Type matchType, List<T> matchValues, Type replaceType) {
        this.matchType = matchType;
        this.matchValues = matchValues;
        this.replaceType = replaceType;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            
            if (!Utils.IsInstance(token, matchType)) continue;
            
            Unit<T> unit = ((Unit<T>)token);
            T value = unit.GetValue();
            foreach (T matchValue in matchValues) {
                if (matchValue.Equals(value)) {
                    List<IToken> replacement = new List<IToken> {
                        (Unit<T>)Activator.CreateInstance(
                            replaceType, new object[] {value}
                        )
                    };
                    List<IToken> replaced = new List<IToken> {token};
                    return new Match(i, i, replacement, replaced);
                }
            }
        }
        return null;
    }
}
public class VarDeclareMatcher : IMatcher {
    Type varType;
    Type declareType;
    Type type_Type;
    Type varDeclareType;
    
    public VarDeclareMatcher(Type varType, Type declareType, Type type_Type,
                             Type varDeclareType) {
        this.varType = varType;
        this.declareType = declareType;
        this.type_Type = type_Type;
        this.varDeclareType = varDeclareType;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count-2; i++) {
            IToken a = tokens[i];
            IToken b = tokens[i+1];
            IToken c = tokens[i+2];
            if (Utils.IsInstance(a, type_Type) && Utils.IsInstance(b, declareType)
                && Utils.IsInstance(c, varType)) {
                List<IToken> replaced = new List<IToken> {a, b, c};
                IToken result = (IToken)Activator.CreateInstance(
                    varDeclareType, new object[] {
                        new List<IToken> {a, c}
                    }
                );
                return new Match(i, i+2, new List<IToken> {result}, replaced);
            }
        }
        return null;
    }
}
public class FunctionConverterMatcher : IMatcher {
    Type newType;
    
    public FunctionConverterMatcher(Type newType) {
        this.newType = newType;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (token is FunctionHolder) { // TEMP (see TODO.txt)
                RawFuncTemplate raw = ((FuncTemplate)token);
                FuncTemplate template = raw.GetTemplate();
                Block block = raw.GetBlock();
                IToken replacement = (IToken)Activator.CreateInstance(
                    newType, new object[] {
                        template.GetValue(), template.GetArguments(), block
                    }
                );
                return new Match(i, i, new List<IToken> {replacement},
                                 new List<IToken> {token});
            }
        }
        return null;
    }
}
public class ConfigurablePatternExtractor<T> : PatternExtractor<T> {
    public ConfigurablePatternExtractor(List<IPatternSegment> segments, 
                                        PatternProcessor<T> processor) {
        this.segments = segments;
        this.processor = processor;
    }
    public override string ToString() {
        IEnumerable<string> segmentsStrings = segments.Select(
            (IPatternSegment segment) => segment.ToString()
        );
        return Utils.WrapName(
            this.GetType().Name,
            String.Join(", ", segmentsStrings)
        );
    }
}
public interface IPatternSegment {
    bool Matches(IToken token);
}
public class MatcherPatternProcessor : PatternProcessor<Match> {
    PatternProcessor<List<IToken>> subprocessor;
    public MatcherPatternProcessor(PatternProcessor<List<IToken>> subprocessor) {
        this.subprocessor = subprocessor;
    }
    
    protected override Match Process(List<IToken> tokens) {
        throw new NotSupportedException(
            "MatcherPatternProcessor does not support the Process(List<IToken>) method"
        );
    }
    
    public override Match Process(List<IToken> tokens, int start, int end) {
        return new Match(start, end, subprocessor.Process(tokens, start, end), tokens);
    }
}
abstract public class PatternExtractor<T> {
    protected List<IPatternSegment> segments;
    protected PatternProcessor<T> processor;
    
    public T Extract(TreeToken tokens) {
        int maxStart = tokens.Count - segments.Count;
        for (int i = 0; i < maxStart; i++) {
            bool matches = true;
            int j;
            List<IToken> tokenList = new List<IToken>();
            for (j = 0; j < segments.Count; j++) {
                IPatternSegment segment = segments[j];
                IToken token = tokens[i+j];
                tokenList.Add(token);
                if (!segment.Matches(token)) {
                    matches = false;
                    break;
                }
            }
            if (matches) {
                return processor.Process(tokenList, i, i+j);
            }
        }
        return default(T);
    }
}
public abstract class PatternProcessor<T> {
    protected abstract T Process(List<IToken> tokens);
    
    public virtual T Process(List<IToken> tokens, int start, int end) {
        return Process(tokens);
    }
}
public class SlotPatternProcessor : PatternProcessor<List<IToken>> {
    List<int> indices;
    
    public SlotPatternProcessor(List<int> indices) {
        this.indices = indices;
    }
    protected override List<IToken> Process(List<IToken> tokens) {
        List<IToken> result = new List<IToken>();
        foreach (int index in indices) {
            result.Add(tokens[index]);
        }
        return result;
    }
}
public class TextPatternSegment : IPatternSegment {
    string text;
    
    public TextPatternSegment(string text) {
        this.text = text;
    }
    public bool Matches(IToken token) {
        return (token is TextToken 
            && ((TextToken)token).GetText() == text);
    }
}
public class TypePatternSegment : IPatternSegment {
    Type type;
    
    public TypePatternSegment(Type type) {
        this.type = type;
    }
    public bool Matches(IToken token) {
        return Utils.IsInstance(token, type);
    }
}
public class Type_PatternSegment : IPatternSegment {
    Type_ type_;
    
    public Type_PatternSegment(Type_ type_) {
        this.type_ = type_;
    }
    public bool Matches(IToken token) {
        return (token is IValueToken 
            && ((IValueToken)token).GetType_() == type_);
    }
}
public class UnitPatternSegment<T> : IPatternSegment where T : IEquatable<T> {
    T value;
    Type unit;
    
    public UnitPatternSegment(Type unit, T value) {
        this.value = value;
        this.unit = unit;
    }
    public bool Matches(IToken token) {
        return (token is Unit<T> && Utils.IsInstance(token, unit)
            && ((Unit<T>)token).GetValue().Equals(value));
    }
}
public class Constant {}
public class Constants {
    Dictionary<int, Constant> constants = new Dictionary<int, Constant>();
    int counter = 0;
    public int AddConstant(Constant constant) {
        constants[counter] = constant;
        return counter++;
    }
}
public class Field {
    string name;
    Type_ type_;
    
    public Field(string name, Type_ type_) {
        this.name = name;
        this.type_ = type_;
    }
    public Field(VarDeclaration declaration) {
        this.name = declaration.GetName().GetValue();
        this.type_ = declaration.GetType_();
    }
    public string GetName() {
        return name;
    }
    public Type_ GetType_() {
        return type_;
    }
    public override string ToString() {
        return $"{type_}:{name}";
    }
}
public class FloatConstant : Constant {
    string value;
    public FloatConstant(string value) {
        this.value = value;
    }
}
public class IntConstant : Constant {
    string value;
    public IntConstant(string value) {
        this.value = value;
    }
}
public class StringConstant : Constant {
    string value;
    public StringConstant(string value) {
        this.value = value;
    }
}
public class Type_ {
// https://en.wikipedia.org/wiki/Set_(mathematics)#Special_sets_of_numbers_in_mathematics
    public static List<string> BuiltInTypes_ = new List<string> {
        "Unkown",
        "Void",
        "Bool",
        "Byte",
        "W", // whole numbers
        "Z",
        "Q",
        "Array",
        "Struct",
    };
    public static List<KeyValuePair<string, string>> BuiltInTypes_Decent = 
        new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("Unkown", null),
            new KeyValuePair<string, string>("Void", null),
            new KeyValuePair<string, string>("Unkown", null),
            new KeyValuePair<string, string>("Bool", null),
            new KeyValuePair<string, string>("Byte", "W"),
            new KeyValuePair<string, string>("W", "Z"),
            new KeyValuePair<string, string>("Z", "Q"),
            new KeyValuePair<string, string>("Array", null),
            new KeyValuePair<string, string>("Struct", null),
        };
    
    string name;
    List<Type_> generics;
    public Type_(string name, List<Type_> generics) {
        this.name = name;
        this.generics = generics;
    }
    public Type_(string name) {
        this.name = name;
        this.generics = new List<Type_>();
    }
    public Type_ WithGenerics(List<Type_> generics) {
        return new Type_(this.name, generics);
    }
    public string GetName() {
        return name;
    }
    public List<Type_> GetGenerics() {
        return generics;
    }
    public override string ToString() {
        string genericStr = "";
        bool first = true;
        foreach (Type_ generic in generics){
            if (!first) {
                genericStr += ", ";
            }
            genericStr += generic.ToString();
            first = false;
        }
        return Utils.WrapName(name, genericStr, "<", ">");
    }
}
/*
using System;
public enum Primitive {
    Unkown,
    Void,
    Bool,
    Byte,
    W, // unsigned integers
    Z, // signed integers
    Q, // floats
    Bytes,
}
*/
/*
using System;
// To explain: the type of the type_
public enum Type_Type {
    Primitive,
    Struct
}
*/
public class BaseTokenType_ : Unit<string> {
    public BaseTokenType_(string type_) : base(type_) {}
}
public class Block : TreeToken {
    public Block(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new Block(tokens);
    }
}
public class BracketClose : Symbolic {}
public class BracketOpen : Symbolic {}
public class Colon : Symbolic {}
public class Comma : Symbolic {}
public class ConstantValue : Unit<int> {
    public ConstantValue(int constant) : base(constant) {}
}
public class Equal : Symbolic {}
public class FunctionArgumentToken : IToken {
    string name;
    Type_ type_;
    
    public FunctionArgumentToken(string name, Type_ type_) {
        this.name = name;
        this.type_ = type_;
    }
    public string GetName() {
        return name;
    }
    public Type_ GetType_() {
        return type_;
    }
    public override string ToString() {
        return Utils.WrapName(this.GetType().Name, type_.ToString() + ":" + name);
    }
}
public class FunctionToken : IToken {
    PatternExtractor<List<IToken>> pattern;
    List<FunctionArgumentToken> arguments;
    Block block;
    
    public FunctionToken(PatternExtractor<List<IToken>> pattern, 
                         List<FunctionArgumentToken> arguments, Block block) {
        this.pattern = pattern;
        this.arguments = arguments;
        this.block = block;
    }
    public PatternExtractor<List<IToken>> GetPattern() {
        return pattern;
    }
    public List<FunctionArgumentToken> GetArguments() {
        return arguments;
    }
    public Block GetBlock() {
        return block;
    }
    public void SetBlock(Block block) {
        this.block = block;
    }
    public override string ToString() {
        string title = Utils.WrapName(
            this.GetType().Name, String.Join(", ", arguments), "<", ">"
        );
        return Utils.WrapName(title, block.ToString());
    }
}
public class Generics : TreeToken {
    public Generics(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new Generics(tokens);
    }
}
public class GenericsClose : Symbolic {}
public class GenericsOpen : Symbolic {}
public class Holder : TreeToken {
    public Holder(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new Holder(tokens);
    }
    public Block GetBlock() {
        if (this.Count < 2) return null;
        IToken token = this[1];
        if (!(token is Block)) return null;
        return (Block)token;
    }
    public void SetBlock(Block block) {
        if (this.Count < 2)
            throw new InvalidOperationException("Holder does not have block already set");
        this[1] = block;
    }
}
public interface IToken {}
public interface IValueToken : IToken {
    Type_ GetType_();
}
public class Name : Unit<string> {
    public Name(string name) : base(name) {}
}
public class Program : TreeToken { 
    Constants constants;
    List<string> baseTypes_ = null;
    
    public Program(List<IToken> tokens,
                    Constants constants) : base(tokens) {
        this.constants = constants;
    }
    
    public Program(List<IToken> tokens, Constants constants,
                   List<string> baseTypes_) : base(tokens) {
        this.constants = constants;
        this.baseTypes_ = baseTypes_;
    }
    public Constants GetConstants() {
        return constants;
    }
    public List<string> GetBaseTypes_() {
        return baseTypes_;
    }
    public void SetBaseTypes_(List<string> baseTypes_) {
        this.baseTypes_ = baseTypes_;
    }
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new Program(tokens, constants, baseTypes_);
    }
}
public class RawFuncTemplate : TreeToken {
    public RawFuncTemplate(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new RawFuncTemplate(tokens);
    }
}
public class RawFunctionArgument : TreeToken {
    public RawFunctionArgument(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new RawFunctionArgument(tokens);
    }
}
public class Struct : IToken {
    string name;
    List<Field> fields;
    
    public Struct(string name, List<Field> fields) {
        this.name = name;
        this.fields = fields;
    }
    public override string ToString() {
        string result = $"Name: {name}";
        foreach (Field field in fields) {
            result += "\n" + field.ToString();
        }
        return Utils.WrapName(
            "Struct", 
            Utils.WrapNewline(Utils.Indent(result))
        );
    }
}
public class StructHolder : Holder {
    public StructHolder(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new StructHolder(tokens);
    }
}
public class Symbolic : IToken {
    public override string ToString() {
        return "(" + this.GetType().Name + ")";
    }
}
public class TextToken : IToken {
    string text;
    
    public TextToken(string text) {
        this.text = text;
    }
    public string GetText() {
        return text;
    }
    public void SetText(string text) {
        this.text = text;
    }
    public override string ToString() {
        return this.text;
    }
}
public class TokenList : IEnumerator<IToken> {
    List<IToken> tokens;
    int i = -1;
    
    public TokenList(List<IToken> tokens) {
        this.tokens = tokens;
    }
    public bool MoveNext() {
        i++;
        return (i < tokens.Count);
    }
    public void Reset() {
        i = -1;
    }
    public IToken Current {
        get {
            return this.tokens[i];
        }
    }
    public void Dispose() {}
    object IEnumerator.Current {
        get {
            return this.Current;
        }
    }
}
public class TreeToken : IToken, IEnumerable<IToken> {
    List<IToken> tokens;
    
    public TreeToken(List<IToken> tokens) {
        this.tokens = tokens;
    }
    public List<IToken> GetTokens() {
        return tokens;
    }
    public void SetTokens(List<IToken> tokens) {
        this.tokens = tokens;
    }
    public void Add(IToken token) {
        this.tokens.Add(token);
    }
    public IToken this[int i] {
        get {
            return this.tokens[i];
        }
        set {
            this.tokens[i] = value;
        }
    }
    public virtual TreeToken Copy(List<IToken> tokens) {
        return new TreeToken(tokens);
    }
    public TreeToken Copy() {
        return Copy(new List<IToken>(this.tokens));
    }
    public int Count {
        get {
            return this.tokens.Count;
        }
    }
    public IEnumerator<IToken> GetEnumerator() {
        return new TokenList(this.tokens);
    }
    
    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }
    public IEnumerable<IToken> Traverse() {
        yield return this;
        foreach (IToken token in this) {
            if (token is TreeToken) {
                foreach (IToken subToken in ((TreeToken)token).Traverse()) {
                    yield return subToken;
                }
            } else {
                yield return token;
            }
        }
    }
    public IEnumerable<(int, TreeToken)> IndexTraverse() {
        for (int i = 0; i < this.Count; i++) {
            yield return (i, this);
            IToken token = this[i];
            if (token is TreeToken) {
                foreach ((int, TreeToken) sub in ((TreeToken)token).IndexTraverse()) {
                    yield return sub;
                }
            }
        }
    }
    public override string ToString() {
        string result = "";
        bool whitespace = false;
        foreach (IToken token in this) {
            bool whitespaceHere = false;
            if (token is TreeToken 
                || (token is TextToken && 
                ((TextToken)token).GetText() == "\n")
                || Utils.IsInstance(token, typeof(Unit<>))) {
                whitespace = true;
                whitespaceHere = true;
            }
            result += token.ToString();
            if (whitespaceHere) {
                result += "\n";
            }
        }
        result = result.Trim();
        if (whitespace) {
            result = Utils.Indent(result);
            result = Utils.WrapNewline(result);
        }
        return Utils.WrapName(this.GetType().Name, result);
    }
}
public class Type_Token : Unit<Type_> {
    public Type_Token(Type_ type_) : base(type_) {}
}
public class Unit<T> : IToken {
    T value;
    
    public Unit(T value) {
        this.value = value;
    }
    public T GetValue() {
        return value;
    }
    public override string ToString() {
        return Utils.WrapName(this.GetType().Name, this.value.ToString());
    }
}
public class VarDeclaration : TreeToken {
    public VarDeclaration(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new VarDeclaration(tokens);
    }
    public Type_ GetType_() {
        if (this.Count < 2) return null;
        IToken type_token = this[0];
        if (!(type_token is Type_Token)) return null;
        return ((Type_Token)type_token).GetValue();
    }
    public Name GetName() {
        if (this.Count < 2) return null;
        IToken name = this[1];
        if (!(name is Name)) return null;
        return (Name)name;
    }
}
public class FunctionHolder : Holder {
    public FunctionHolder(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new FunctionHolder(tokens);
    }
    public RawFuncTemplate GetRawTemplate() {
        if (this.Count < 2) return null;
        IToken token = this[0];
        if (!(token is RawFuncTemplate)) return null;
        return (RawFuncTemplate)token;
    }
    public RawFuncTemplate GetTemplate() {
        if (this.Count < 2) return null;
        IToken token = this[0];
        if (!(token is FuncTemplate)) return null;
        return (FuncTemplate)token;
    }
    public void SetTemplate(IToken template) {
        if (this.Count < 2)
            throw new InvalidOperationException(
                "FuncHolder does not have template already set"
            );
        this[0] = template;
    }
}
public class FuncTemplate : Unit<PatternExtractor<List<IToken>>> {
    List<FunctionArgumentToken> arguments;
    
    public FuncTemplate(PatternExtractor<List<IToken>> pattern, 
                        List<FunctionArgumentToken> arguments) : base(pattern) {
        this.arguments = arguments;
    }
    public List<FunctionArgumentToken> GetArguments() {
        return arguments;
    }
}

namespace Epsilon;
public class ExternalFunction : FunctionDeclaration {
    readonly PatternExtractor<List<IToken>> pattern;
    readonly List<FunctionArgument> arguments;
    readonly string id;
    readonly Func<List<IValueToken>, Type_> returnType_;
    readonly FunctionSource source;
    readonly bool doesReturnVoid = false;

    public ExternalFunction(PatternExtractor<List<IToken>> pattern, List<FunctionArgument> arguments, string id, Func<List<Type_>, Type_> returnType_, FunctionSource source, bool doesReturnVoid=false) {
        this.pattern = pattern;
        this.arguments = arguments;
        this.id = id;
        this.doesReturnVoid = doesReturnVoid;
        this.returnType_ = (tokens) => {
            List<Type_> types_ = tokens.Select(token=>token.GetType_()).ToList();
            try {
                return returnType_(types_);
            } catch (FunctionCallTypes_Exception e) {
                throw new SyntaxErrorException(
                    e.Message, tokens[e.ArgumentIndex]
                );
            }
        };
        this.source = source;
    }

    public ExternalFunction(PatternExtractor<List<IToken>> pattern, List<FunctionArgument> arguments, string id, Type_ returnType_, FunctionSource source) {
        this.pattern = pattern;
        this.arguments = arguments;
        this.id = id;
        doesReturnVoid = returnType_ == null;
        this.returnType_ = (tokens) => returnType_;
        this.source = source;
    }

    public ExternalFunction(PatternExtractor<List<IToken>> pattern, List<FunctionArgument> arguments, string id, FunctionSource source) {
        this.pattern = pattern;
        this.arguments = arguments;
        this.id = id;
        doesReturnVoid = true;
        returnType_ = (tokens) => null;
        this.source = source;
    }

    public override PatternExtractor<List<IToken>> GetPattern() {
        return pattern;
    }

    public override List<FunctionArgument> GetArguments() {
        return arguments;
    }

    protected override Type_ _GetReturnType_(List<IValueToken> tokens) {
        return returnType_(tokens);
    }

    public override string GetID() {
        return id;
    }

    public override bool DoesReturnVoid() {
        return doesReturnVoid;
    }

    public override FunctionSource GetSource() {
        return source;
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, id.ToString());
    }
}

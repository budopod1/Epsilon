namespace Epsilon;
public class RealExternalFunction(PatternExtractor<List<IToken>> pattern, List<FunctionArgument> arguments, string id, string sourcePath, string callee, Type_ returnType_, bool doesReturnVoid, FunctionSource source, bool takesOwnership = false, bool resultInParams = false) : RealFunctionDeclaration {
    readonly PatternExtractor<List<IToken>> pattern = pattern;
    readonly List<FunctionArgument> arguments = arguments;
    readonly string id = id;
    readonly string sourcePath = sourcePath;
    readonly string callee = callee;
    readonly Type_ returnType_ = returnType_;
    readonly bool doesReturnVoid = doesReturnVoid;
    readonly FunctionSource source = source;
    readonly bool takesOwnership = takesOwnership;
    readonly bool resultInParams = resultInParams;

    public override PatternExtractor<List<IToken>> GetPattern() {
        return pattern;
    }

    public override List<FunctionArgument> GetArguments() {
        return arguments;
    }

    protected override Type_ _GetReturnType_() {
        return returnType_;
    }

    public override bool DoesReturnVoid() {
        return doesReturnVoid;
    }

    public override string GetID() {
        return id;
    }

    public override FunctionSource GetSource() {
        return source;
    }

    public override string GetSourcePath() {
        return sourcePath;
    }

    public override string GetCallee() {
        return callee;
    }

    public override bool TakesOwnership() {
        return takesOwnership;
    }

    public override bool ResultInParams() {
        return resultInParams;
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, id.ToString());
    }
}

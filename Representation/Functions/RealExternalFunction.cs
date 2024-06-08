using System;
using System.Linq;
using System.Collections.Generic;

public class RealExternalFunction : RealFunctionDeclaration {
    PatternExtractor<List<IToken>> pattern;
    List<FunctionArgument> arguments;
    string id;
    string sourcePath;
    string callee;
    Type_ returnType_;
    bool doesReturnVoid;
    FunctionSource source;
    bool takesOwnership;
    bool resultInParams;

    public RealExternalFunction(PatternExtractor<List<IToken>> pattern, List<FunctionArgument> arguments, string id, string sourcePath, string callee, Type_ returnType_, bool doesReturnVoid, FunctionSource source, bool takesOwnership=false, bool resultInParams=false) {
        this.pattern = pattern;
        this.arguments = arguments;
        this.id = id;
        this.sourcePath = sourcePath;
        this.callee = callee;
        this.returnType_ = returnType_;
        this.doesReturnVoid = doesReturnVoid;
        this.source = source;
        this.takesOwnership = takesOwnership;
        this.resultInParams = resultInParams;
    }

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

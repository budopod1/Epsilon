using System;
using System.Linq;
using System.Collections.Generic;

public class RealExternalFunction : RealFunctionDeclaration {
    PatternExtractor<List<IToken>> pattern;
    List<FunctionArgument> arguments;
    string id;
    Type_ returnType_;

    public RealExternalFunction(PatternExtractor<List<IToken>> pattern, List<FunctionArgument> arguments, string id, Type_ returnType_) {
        this.pattern = pattern;
        this.arguments = arguments;
        this.id = id;
        this.returnType_ = returnType_;
    }

    public override PatternExtractor<List<IToken>> GetPattern() {
        return pattern;
    }

    public override List<FunctionArgument> GetArguments() {
        return arguments;
    }

    public override Type_ GetReturnType_(List<IValueToken> tokens) {
        return returnType_;
    }

    public override Type_ GetReturnType_() {
        return returnType_;
    }

    public override string GetID() {
        return id;
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, id.ToString());
    }
}

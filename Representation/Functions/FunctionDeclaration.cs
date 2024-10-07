using System;
using System.Collections.Generic;

public abstract class FunctionDeclaration {
    public abstract PatternExtractor<List<IToken>> GetPattern();
    public abstract List<FunctionArgument> GetArguments();
    public abstract string GetID();
    public abstract FunctionSource GetSource();
    public abstract bool DoesReturnVoid();
    protected abstract Type_ _GetReturnType_(List<IValueToken> tokens);

    public Type_ GetReturnType_(List<IValueToken> tokens) {
        if (DoesReturnVoid()) {
            throw new InvalidOperationException("Void function has no return type_");
        } else {
            return _GetReturnType_(tokens);
        }
    }

    public void VerifyPassedTokens(List<IValueToken> tokens) {
        _GetReturnType_(tokens);
    }
}

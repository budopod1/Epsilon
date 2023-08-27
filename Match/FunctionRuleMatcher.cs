using System;
using System.Reflection;
using System.Collections.Generic;

public class FunctionRuleMatcher : IMatcher {
    PatternExtractor<List<IToken>> extractor;
    Function func;
    Type functionCall;
    
    public FunctionRuleMatcher(Function func, Type functionCall) {
        this.extractor = func.GetPattern();
        this.functionCall = functionCall;
        this.func = func;
    }
    
    public Match Match(IParentToken tokens) {
        int start = -1;
        int end = -1;
        List<IToken> replaced = new List<IToken>();
        extractor.SetCallback((List<IToken> replaced_, int start_, int end_) => {
            replaced = replaced_;
            start = start_;
            end = end_;
        });
        List<IToken> arguments = extractor.Extract(tokens);
        if (arguments == null) return null;
        IToken call = (IToken)Activator.CreateInstance(
            functionCall, new object[] {extractor.GetSegments(), arguments}
        );
        return new Match(start, end, new List<IToken> {call}, replaced);
    }
}

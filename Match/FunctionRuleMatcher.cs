using System;
using System.Reflection;
using System.Collections.Generic;

public class FunctionRuleMatcher : IMatcher {
    PatternExtractor<List<IToken>> extractor;
    Function func;
    
    public FunctionRuleMatcher(Function func) {
        this.extractor = func.GetPattern();
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
        return new Match(start, end, new List<IToken> {
            new RawFunctionCall(extractor.GetSegments(), arguments)
        }, replaced);
    }
}

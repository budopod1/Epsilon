using System;
using System.Collections.Generic;

public class FuncTemplate : Unit<PatternExtractor<List<IToken>>>,
                            IMultiLineToken, IBarMatchingInto {
    List<FunctionArgumentToken> arguments;

    public FuncTemplate(PatternExtractor<List<IToken>> pattern,
                        List<FunctionArgumentToken> arguments) : base(pattern) {
        this.arguments = arguments;
    }

    public List<FunctionArgumentToken> GetArguments() {
        return arguments;
    }
}

using System;
using System.Collections.Generic;

public class FuncTemplate : Unit<PatternExtractor<List<Token>>>, IMultiLineToken {
    List<FunctionArgumentToken> arguments;
    
    public FuncTemplate(PatternExtractor<List<Token>> pattern, 
                        List<FunctionArgumentToken> arguments) : base(pattern) {
        this.arguments = arguments;
    }

    public List<FunctionArgumentToken> GetArguments() {
        return arguments;
    }
}

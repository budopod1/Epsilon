using System;
using System.Collections.Generic;

public interface IFunctionDeclaration {
    PatternExtractor<List<IToken>> GetPattern();
    List<FunctionArgument> GetArguments();
    int GetID();
    Type_ GetReturnType_(List<IValueToken> tokens);
}

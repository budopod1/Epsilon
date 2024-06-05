using System;

public interface IFunctionCall : IToken {
    FunctionDeclaration GetFunction();
}

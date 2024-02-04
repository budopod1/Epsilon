using System;
using System.Collections.Generic;

public abstract class RealFunctionDeclaration : FunctionDeclaration {
    public abstract Type_ GetReturnType_();
    public abstract string GetCallee();
}

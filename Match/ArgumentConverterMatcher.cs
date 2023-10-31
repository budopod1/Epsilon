using System;
using System.Reflection;
using System.Collections.Generic;

public class ArgumentConverterMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        for (int j = 0; j < tokens.Count; j++) {
            IToken token = tokens[j];
            if (token is RawFunctionArgument) {
                Unit<string> name = null;
                Unit<Type_> type_Token = null;
                IParentToken tree = ((IParentToken)token);
                for (int i = 0; i < tree.Count; i++) {
                    IToken subtoken = tree[i];
                    if (subtoken is Name) {
                        name = ((Unit<string>)subtoken);
                    } else if (subtoken is Type_Token) {
                        type_Token = ((Unit<Type_>)subtoken);
                    }
                }
                if (name == null || type_Token == null) {
                    throw new SyntaxErrorException(
                        "Function argument is incomplete", token
                    );
                }
                IToken replacement = new FunctionArgumentToken(
                    name.GetValue(), type_Token.GetValue()
                );
                return new Match(
                    j, j, new List<IToken> {replacement},
                    new List<IToken> {token}
                );
            }
        }
        return null;
    }
}

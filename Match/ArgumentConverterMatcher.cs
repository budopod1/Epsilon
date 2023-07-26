using System;
using System.Reflection;
using System.Collections.Generic;

public class ArgumentConverterMatcher : IMatcher {
    Type oldArgument;
    Type nameType;
    Type type_TokenType;
    Type newArgument;
    
    public ArgumentConverterMatcher(Type oldArgument, Type name, Type type_Token,
                                    Type newArgument) {
        this.oldArgument = oldArgument;
        nameType = name;
        type_TokenType = type_Token;
        this.newArgument = newArgument;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (Utils.IsInstance(token, oldArgument)) {
                Unit<string> name = null;
                Unit<Type_> type_Token = null;
                foreach (IToken subtoken in (TreeToken)token) {
                    if (Utils.IsInstance(subtoken, nameType)) {
                        name = ((Unit<string>)subtoken);
                    } else if (Utils.IsInstance(subtoken, type_TokenType)) {
                        type_Token = ((Unit<Type_>)subtoken);
                    }
                }
                if (name == null || type_Token == null) {
                    throw new InvalidOperationException(
                        "RawFunctionArgument is incomplete"
                    );
                }
                IToken replacement = (IToken)Activator.CreateInstance(
                    newArgument, new object[] {
                        name.GetValue(), type_Token.GetValue()
                    }
                );
                return new Match(i, i, new List<IToken> {replacement},
                                 new List<IToken> {token});
            }
        }
        return null;
    }
}

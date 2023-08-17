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
    
    public Match Match(ParentToken tokens) {
        for (int j = 0; j < tokens.Count; j++) {
            Token token = tokens[j];
            if (Utils.IsInstance(token, oldArgument)) {
                Unit<string> name = null;
                Unit<Type_> type_Token = null;
                ParentToken tree = ((ParentToken)token);
                for (int i = 0; i < tree.Count; i++) {
                    Token subtoken = tree[i];
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
                Token replacement = (Token)Activator.CreateInstance(
                    newArgument, new object[] {
                        name.GetValue(), type_Token.GetValue()
                    }
                );
                return new Match(j, j, new List<Token> {replacement},
                                 new List<Token> {token});
            }
        }
        return null;
    }
}

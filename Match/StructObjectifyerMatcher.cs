using System;
using System.Reflection;
using System.Collections.Generic;

public class StructObjectifyerMatcher : IMatcher {
    Type structHolderType;
    Type structCompiledType;
    ListTokenParser<Field> listParser;
    
    public StructObjectifyerMatcher(Type structHolderType, Type structCompiledType,
                               ListTokenParser<Field> listParser) {
        this.structHolderType = structHolderType;
        this.structCompiledType = structCompiledType;
        this.listParser = listParser;
    }
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (Utils.IsInstance(token, structHolderType)) {
                Holder holder = ((Holder)token);
                Block block = holder.GetBlock();
                if (block == null) continue;
                IToken nameT = holder[0];
                if (!(nameT is Unit<string>)) continue;
                Name name = ((Name)nameT);
                string nameStr = name.GetValue();
                List<Field> fields = listParser.Parse(block);
                if (fields == null) {
                    throw new SyntaxErrorException(
                        "Malformed struct", token
                    );
                }
                IToken compiled = (IToken)Activator.CreateInstance(
                    structCompiledType, new object[] {
                        nameStr, fields
                    }
                );
                List<IToken> replaced = new List<IToken> {token};
                return new Match(i, i, new List<IToken> {compiled}, replaced);
            }
        }
        return null;
    }
}

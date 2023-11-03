using System;
using System.Reflection;
using System.Collections.Generic;

public class StructObjectifyerMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        ListTokenParser<Field> listParser = new ListTokenParser<Field>(
            new TextPatternSegment(","), typeof(VarDeclaration), 
            (token) => new Field((VarDeclaration)token)
        );
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (token is StructHolder) {
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
                List<IToken> replaced = new List<IToken> {token};
                return new Match(i, i, new List<IToken> {
                    new Struct(nameStr, fields)
                }, replaced);
            }
        }
        return null;
    }
}

using System;
using System.Linq;
using System.Collections.Generic;

public class StringMatcher : IMatcher {
    Program program;

    public StringMatcher(Program program) {
        this.program = program;
    }

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (!(token is TextToken) ||
                ((TextToken)token).GetText() != "\"") {
                continue;
            }

            List<IToken> matched = new List<IToken>();
            matched.Add(token);
            bool wasBackslash = false;
            for (int j = i + 1; j < tokens.Count; j++) {
                token = tokens[j];
                if (!(token is TextToken)) {
                    continue;
                }
                matched.Add(token);
                string text = ((TextToken)token).GetText();

                if (wasBackslash) {
                    wasBackslash = false;
                    continue;
                }
                if (text == "\\") {
                    wasBackslash = true;
                    continue;
                }
                if (text != "\"") continue;

                string matchedString = String.Join("", matched.Select(
                        (IToken sub) => ((TextToken)sub).GetText()
                ));

                try {
                    List<IToken> replacement = new List<IToken> {
                        new ConstantValue(
                            StringConstant.FromString(matchedString)
                        )
                    };

                    return new Match(i, j, replacement, matched);
                } catch (LiteralDecodeException e) {
                    throw new SyntaxErrorException(
                        e.Message, new CodeSpan(e.Position+matched[0].span.GetStart())
                    );
                }
            }
        }
        return null;
    }
}

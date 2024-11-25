namespace Epsilon;
public class FunctionArgumentMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        int indent = 0;
        List<IToken> replaced = null;

        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];

            if (token is TextToken ttoken) {
                string text = ttoken.GetText();
                if (text == "[") {
                    if (indent == 0) {
                        replaced = [];
                    }
                    indent++;
                } else if (text == "]") {
                    indent--;
                    if (indent < 0) {
                        throw new SyntaxErrorException(
                            "Closing square bracket does not have a matching opening square bracket", token
                        );
                    } else if (indent == 0) {
                        return new Match(i-replaced.Count, i, [
                            new RawFunctionArgument(replaced.Skip(1).ToList())
                        ], replaced);
                    }
                }
            }

            replaced?.Add(token);
        }

        if (indent > 0) {
            throw new SyntaxErrorException(
                "Expected closing square backet",
                new CodeSpan(tokens[^1].span.GetEnd())
            );
        }

        return null;
    }
}

using CsJSONTools;

namespace Epsilon;
public static class InitialTokenizer {
    enum ParseState {
        NORMAL,
        LINECOMMENT,
        BLOCKCOMMENT,
        STRINGCONSTANT,
        CHARCONSTANT
    }

    public static IEnumerable<IToken> Tokenize(string str) {
        List<IToken> result = [];

        bool isEscaped = false;
        int constantStart = -1;
        ParseState state = ParseState.NORMAL;

        for (int i = 0; i < str.Length; i++) {
            char chr = str[i];

            bool wasSlash = i > 0 && str[i-1] == '/';
            bool wasStar = i > 0 && str[i-1] == '*';

            switch (state) {
            case ParseState.NORMAL:
                if (wasSlash && chr == '/') {
                    state = ParseState.LINECOMMENT;
                    result.RemoveAt(result.Count-1);
                } else if (wasSlash && chr == '*') {
                    state = ParseState.BLOCKCOMMENT;
                    result.RemoveAt(result.Count-1);
                } else if (chr == '\'') {
                    state = ParseState.CHARCONSTANT;
                    constantStart = i;
                } else if (chr == '"') {
                    state = ParseState.STRINGCONSTANT;
                    constantStart = i;
                } else {
                    TextToken token = new(chr.ToString());
                    token.span = new CodeSpan(i);
                    result.Add(token);
                }
                break;
            case ParseState.LINECOMMENT:
                if (chr == '\n') {
                    state = ParseState.NORMAL;
                    i--;
                }
                break;
            case ParseState.BLOCKCOMMENT:
                if (chr == '/' && wasStar) {
                    state = ParseState.NORMAL;
                }
                break;
            case ParseState.STRINGCONSTANT:
                if (!isEscaped && chr == '"') {
                    string rawTxt = str.Substring(constantStart, i - constantStart + 1);

                    ConstantValue token;
                    try {
                        token = new(StringConstant.FromString(rawTxt));
                    } catch (LiteralDecodeException e) {
                        throw new SyntaxErrorException(
                            e.Message, new CodeSpan(constantStart, i)
                        );
                    }
                    token.span = new CodeSpan(constantStart, i);
                    result.Add(token);

                    state = ParseState.NORMAL;
                }
                break;
            case ParseState.CHARCONSTANT:
                if (!isEscaped && chr == '\'') {
                    string rawTxt = str.Substring(constantStart, i - constantStart + 1);

                    ConstantValue token;
                    try {
                        token = new(CharConstant.FromString(rawTxt));
                    } catch (Exception e) when (
                        e is LiteralDecodeException || e is ArgumentException
                    ) {
                        throw new SyntaxErrorException(
                            e.Message, new CodeSpan(constantStart, i)
                        );
                    }
                    token.span = new CodeSpan(constantStart, i);
                    result.Add(token);

                    state = ParseState.NORMAL;
                }
                break;
            default:
                throw new InvalidOperationException();
            }

            if (state == ParseState.STRINGCONSTANT || state == ParseState.CHARCONSTANT) {
                if (isEscaped) {
                    isEscaped = false;
                } else if (chr == '\\') {
                    isEscaped = true;
                }
            } else {
                isEscaped = false;
            }
        }

        return result;
    }
}

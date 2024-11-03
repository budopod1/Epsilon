namespace Epsilon;
public class RawFuncSignatureMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        bool wasNL = true;
        for (int i = 0; i < tokens.Count; i++) {
            IToken stoken = tokens[i];
            if (wasNL) {
                List<IToken> matched = [];
                List<IToken> before = [];
                List<IToken> after = [];
                bool hasHashtag = false;
                for (int j = i; j < tokens.Count; j++) {
                    IToken token = tokens[j];
                    if (token is TextToken ttoken2) {
                        string txt = ttoken2.GetText();
                        if (txt == "#") {
                            hasHashtag = true;
                            matched.Add(token);
                            continue;
                        } else if (txt == "{") {
                            if (hasHashtag) {
                                if (after.Count == 0) {
                                    throw new SyntaxErrorException(
                                        "Function template cannot be empty", TokenUtils.MergeSpans(matched)
                                    );
                                }
                                RawFuncReturnType_ ret = new(before);
                                ret.span = TokenUtils.MergeSpans(before);
                                RawFuncTemplate template = new(after);
                                template.span = TokenUtils.MergeSpans(after);
                                return new Match(
                                    i, j - 1, [
                                        new RawFuncSignature(
                                            ret,
                                            template
                                        )
                                    ], matched
                                );
                            } else {
                                break;
                            }
                        } else if (txt == "\n") {
                            break;
                        }
                    }
                    if (hasHashtag) {
                        after.Add(token);
                    } else {
                        before.Add(token);
                    }
                    matched.Add(token);
                }
            }
            if (stoken is TextToken ttoken && ttoken.GetText() == "\n") {
                wasNL = true;
            } else {
                wasNL = false;
            }
        }
        return null;
    }
}

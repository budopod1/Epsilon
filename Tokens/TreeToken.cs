using System;
using System.Collections;
using System.Collections.Generic;

public class TreeToken : IParentToken, IEnumerable<IToken> {
    public IParentToken parent { get; set; }
    
    List<IToken> tokens;
    
    public TreeToken(List<IToken> tokens) {
        this.tokens = tokens;
    }

    public List<IToken> GetTokens() {
        return tokens;
    }

    public void SetTokens(List<IToken> tokens) {
        this.tokens = tokens;
    }

    public void Add(IToken token) {
        this.tokens.Add(token);
    }

    public IToken this[int i] {
        get {
            return this.tokens[i];
        }

        set {
            this.tokens[i] = value;
        }
    }

    protected virtual TreeToken Copy_(List<IToken> tokens) {
        return new TreeToken(tokens);
    }

    public TreeToken Copy(List<IToken> tokens) {
        TreeToken copy = Copy_(tokens);
        copy.parent = parent;
        return copy;
    }

    public TreeToken Copy() {
        TreeToken copy = Copy_(new List<IToken>(this.tokens));
        copy.parent = parent;
        return copy;
    }

    public int Count {
        get {
            return this.tokens.Count;
        }
    }

    public IEnumerator<IToken> GetEnumerator() {
        return new TokenList(this.tokens);
    }
    
    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }

    public IEnumerable<IToken> Traverse() {
        yield return this;
        foreach (IToken token in this) {
            if (token is TreeToken) {
                foreach (IToken subToken in ((TreeToken)token).Traverse()) {
                    yield return subToken;
                }
            } else {
                yield return token;
            }
        }
    }

    public IEnumerable<(int, TreeToken)> IndexTraverse() {
        for (int i = 0; i < this.Count; i++) {
            yield return (i, this);
            IToken token = this[i];
            if (token is TreeToken) {
                foreach ((int, TreeToken) sub in ((TreeToken)token).IndexTraverse()) {
                    yield return sub;
                }
            }
        }
    }

    public override string ToString() {
        string result = "";
        bool whitespace = false;
        foreach (IToken token in this) {
            result += token.ToString();
            if (token is IMultiLineToken 
                || (token is TextToken && 
                ((TextToken)token).GetText() == "\n")
                || Utils.IsInstance(token, typeof(Unit<>))) {
                whitespace = true;
                result += "\n";
            }
        }
        result = result.Trim();
        if (whitespace) {
            result = Utils.Indent(result);
            result = Utils.WrapNewline(result);
        }
        return Utils.WrapName(this.GetType().Name, result);
    }
}

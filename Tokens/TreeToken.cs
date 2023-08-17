using System;
using System.Collections;
using System.Collections.Generic;

public class TreeToken : ParentToken, IEnumerable<Token> {
    List<Token> tokens;
    
    public TreeToken(List<Token> tokens) {
        this.tokens = tokens;
    }

    public List<Token> GetTokens() {
        return tokens;
    }

    public void SetTokens(List<Token> tokens) {
        this.tokens = tokens;
    }

    public void Add(Token token) {
        this.tokens.Add(token);
    }

    public override Token this[int i] {
        get {
            return this.tokens[i];
        }

        set {
            this.tokens[i] = value;
        }
    }

    public virtual TreeToken Copy(List<Token> tokens) {
        return new TreeToken(tokens);
    }

    public TreeToken Copy() {
        return Copy(new List<Token>(this.tokens));
    }

    public override int Count {
        get {
            return this.tokens.Count;
        }
    }

    public IEnumerator<Token> GetEnumerator() {
        return new TokenList(this.tokens);
    }
    
    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }

    public IEnumerable<Token> Traverse() {
        yield return this;
        foreach (Token token in this) {
            if (token is TreeToken) {
                foreach (Token subToken in ((TreeToken)token).Traverse()) {
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
            Token token = this[i];
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
        foreach (Token token in this) {
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

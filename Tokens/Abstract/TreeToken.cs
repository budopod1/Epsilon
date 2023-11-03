using System;
using System.Collections;
using System.Collections.Generic;

public abstract class TreeToken : IParentToken, IEnumerable<IToken> {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
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
        tokens.Add(token);
    }

    public IToken this[int i] {
        get {
            return tokens[i];
        }

        set {
            tokens[i] = value;
        }
    }

    protected abstract TreeToken _Copy(List<IToken> tokens);

    public TreeToken Copy(List<IToken> tokens) {
        TreeToken copy = _Copy(tokens);
        copy.parent = parent;
        copy.span = span;
        return copy;
    }

    public TreeToken Copy() {
        TreeToken copy = _Copy(new List<IToken>(tokens));
        copy.parent = parent;
        return copy;
    }

    public int Count {
        get {
            return tokens.Count;
        }
    }

    public IEnumerator<IToken> GetEnumerator() {
        return new TokenList(tokens);
    }
    
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
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
        return Utils.WrapName(GetType().Name, result);
    }
}

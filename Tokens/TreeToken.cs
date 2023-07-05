using System;
using System.Collections;
using System.Collections.Generic;

public class TreeToken : IToken, IEnumerable<IToken> {
    public List<IToken> tokens;
    
    public TreeToken(List<IToken> tokens) {
        this.tokens = tokens;
    }

    public void Add(IToken token) {
        this.tokens.Add(token);
    }

    public IToken this[int i] {
        get {
            return this.tokens[i];
        }
    }

    // create copy method

    public virtual TreeToken Copy(List<IToken> tokens) {
        return new TreeToken(tokens);
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

    public override string ToString() {
        string result = "";
        bool whitespace = false;
        foreach (IToken token in this) {
            bool whitespaceHere = false;
            if (token is TreeToken || 
                (token is TextToken && 
                ((TextToken)token).Text == "\n")
                || token is Unit) {
                whitespace = true;
                whitespaceHere = true;
            }
            result += token.ToString();
            if (whitespaceHere) {
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

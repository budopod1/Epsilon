using System;
using System.Collections.Generic;

public class Match {
    int start;
    int end;
    List<IToken> replacement;
    List<IToken> matched;
    
    public Match(int start, int end, List<IToken> replacement,
                 List<IToken> matched) {
        this.start = start;
        this.end = end;
        this.replacement = replacement;
        this.matched = matched;
        // Console.WriteLine(this.ToString());
    }

    public TreeToken Replace(TreeToken tokens) {
        List<IToken> result = new List<IToken>();
        int i = 0;
        foreach (IToken token in tokens) {
            if (i > this.end || i < this.start) {
                result.Add(token);
            } else if (i == this.start) {
                result.AddRange(this.replacement);
            }
            i++;
        }
        return tokens.Copy(result);
    }

    public void SetReplacement(List<IToken> replacement) {
        this.replacement = replacement;
    }

    public List<IToken> GetMatched() {
        return this.matched;
    }

    public override string ToString() {
        string result = "(" + this.start.ToString();
        result += ", " + this.end.ToString() + "):";
        foreach (IToken token in this.matched) {
            result += token.ToString();
        }
        result += "|";
        foreach (IToken token in this.replacement) {
            result += token.ToString();
        }
        return Utils.WrapName(this.GetType().Name, result);
    }
}

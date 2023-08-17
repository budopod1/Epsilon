using System;
using System.Collections.Generic;
using System.Reflection;

public class Match {
    int start;
    int end;
    List<Token> replacement;
    List<Token> matched;
    
    public Match(int start, int end, List<Token> replacement,
                 List<Token> matched) {
        this.start = start;
        this.end = end;
        this.replacement = replacement;
        this.matched = matched;
        // Console.WriteLine(this.ToString());
    }

    public TreeToken Replace(TreeToken tokens) {
        List<Token> result = new List<Token>();
        int i = 0;
        foreach (Token token in tokens) {
            if (i > this.end || i < this.start) {
                result.Add(token);
            } else if (i == this.start) {
                result.AddRange(this.replacement);
            }
            i++;
        }
        return tokens.Copy(result);
    }

    public void SetReplacement(List<Token> replacement) {
        this.replacement = replacement;
    }

    public List<Token> GetMatched() {
        return this.matched;
    }

    public override string ToString() {
        string result = "(" + this.start.ToString();
        result += ", " + this.end.ToString() + "):";
        for (int i = 0; i < this.matched.Count; i++) {
            result += this.matched[i].ToString();
        }
        result += "|";
        for (int i = 0; i < this.replacement.Count; i++) {
            result += this.replacement[i].ToString();
        }
        return Utils.WrapName(this.GetType().Name, result);
    }
}

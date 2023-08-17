using System;
using System.Reflection;
using System.Collections.Generic;

public class BlockMatcher : IMatcher {
    Type start;
    Type end;
    Type holder;
    
    public BlockMatcher(Type start, Type end, Type holder) {
        this.start = start;
        this.end = end;
        this.holder = holder;
    }
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            int indent = 0;
            bool any = false;
            List<Token> replaced = new List<Token>();
            for (int j = i; j < tokens.Count; j++) {
                Token token = tokens[j];
                replaced.Add(token);
                if (Utils.IsInstance(token, start)) {
                    indent++;
                } else if (Utils.IsInstance(token, end)) {
                    if (!any) {
                        break;
                    }
                    indent--;
                }
                if (indent == 0) {
                    if (any) {
                        List<Token> replacement = new List<Token>();
                        List<Token> replace = new List<Token>(replaced);
                        replace.RemoveAt(0);
                        replace.RemoveAt(replace.Count-1);
                        Token holderToken = (Token)Activator.CreateInstance(holder, new object[] {replace});
                        replacement.Add(holderToken);
                        return new Match(i, j, replacement, replaced);
                    } else {
                        break;
                    }
                }
                any = true;
            }
        }
        return null;
    }
}

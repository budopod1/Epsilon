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
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            int indent = 0;
            bool any = false;
            List<IToken> replaced = new List<IToken>();
            for (int j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
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
                    if (!any) break;
                    List<IToken> replacement = new List<IToken>();
                    List<IToken> replace = new List<IToken>(replaced);
                    replace.RemoveAt(0);
                    replace.RemoveAt(replace.Count-1);
                    IToken holderToken = (IToken)Activator.CreateInstance(holder, new object[] {replace});
                    replacement.Add(holderToken);
                    return new Match(i, j, replacement, replaced);
                }
                any = true;
            }
            any = false;
        }
        return null;
    }
}

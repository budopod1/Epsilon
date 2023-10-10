using System;
using System.Collections.Generic;

public class ListTokenParser<T> {
    enum ParseState {
        EXPECTITEM,
        EXPECTSEPARATOR
    }
    
    IPatternSegment seperator;
    Type item;
    Func<IToken, T> parser;
    
    public ListTokenParser(IPatternSegment seperator, Type item, Func<IToken, T> parser) {
        this.seperator = seperator;
        this.item = item;
        this.parser = parser;
    }

    public List<T> Parse(IParentToken tree) {
        List<T> list = new List<T>();
        ParseState state = ParseState.EXPECTITEM;
        for (int i = 0; i < tree.Count; i++) {
            IToken token = tree[i];
            switch (state) {
                case ParseState.EXPECTITEM:
                    if (Utils.IsInstance(token, item)) {
                        list.Add(parser(token));
                        state = ParseState.EXPECTSEPARATOR;
                    } else {
                        return null;
                    }
                    break;
                case ParseState.EXPECTSEPARATOR:
                    if (seperator.Matches(token)) {
                        state = ParseState.EXPECTITEM;
                    } else {
                        return null;
                    }
                    break;
            }
        }
        return list;
    }
}

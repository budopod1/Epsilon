using System;
using System.Collections.Generic;

public class ListTokenParser<T> {
    enum ParseState {
        ExpectItem,
        ExpectSeperator
    }
    
    Type seperator;
    Type item;
    Func<IToken, T> parser;
    
    public ListTokenParser(Type seperator, Type item, Func<IToken, T> parser) {
        this.seperator = seperator;
        this.item = item;
        this.parser = parser;
    }

    public List<T> Parse(TreeToken tree) {
        List<T> list = new List<T>();
        ParseState state = ParseState.ExpectItem;
        foreach (IToken token in tree) {
            switch (state) {
                case ParseState.ExpectItem:
                    if (Utils.IsInstance(token, item)) {
                        list.Add(parser(token));
                        state = ParseState.ExpectSeperator;
                    } else {
                        return null;
                    }
                    break;
                case ParseState.ExpectSeperator:
                    if (Utils.IsInstance(token, seperator)) {
                        state = ParseState.ExpectItem;
                    } else {
                        return null;
                    }
                    break;
            }
        }
        return list;
    }
}

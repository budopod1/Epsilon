using System;
using System.Collections.Generic;

public class ValueListItem : TreeToken {
    public ValueListItem(List<IToken> tokens) : base(tokens) {}

    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new ValueListItem(tokens);
    }
}

using System;
using System.Collections.Generic;

public class EmptyPatternProcessor : IPatternProcessor<List<IToken>> {
    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        return [];
    }
}

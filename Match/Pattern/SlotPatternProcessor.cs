using System;
using System.Collections.Generic;

public class SlotPatternProcessor : PatternProcessor<List<IToken>> {
    List<int> indices;
    
    public SlotPatternProcessor(List<int> indices) {
        this.indices = indices;
    }

    protected override List<IToken> Process(List<IToken> tokens) {
        List<IToken> result = new List<IToken>();
        foreach (int index in indices) {
            result.Add(tokens[index]);
        }
        return result;
    }
}

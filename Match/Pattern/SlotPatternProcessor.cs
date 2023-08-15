using System;
using System.Collections.Generic;

public class SlotPatternProcessor : IPatternProcessor<List<IToken>> {
    List<int> indices;
    
    public SlotPatternProcessor(List<int> indices) {
        this.indices = indices;
    }

    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        List<IToken> result = new List<IToken>();
        foreach (int index in indices) {
            result.Add(tokens[index]);
        }
        return result;
    }
}

using System;
using System.Collections.Generic;

public class SlotCastPatternProcessor<T> : IPatternProcessor<List<T>> {
    List<int> indices;
    
    public SlotCastPatternProcessor(List<int> indices) {
        this.indices = indices;
    }

    public List<T> Process(List<IToken> tokens, int start, int end) {
        List<T> result = new List<T>();
        foreach (int index in indices) {
            result.Add((T)(tokens[index]));
        }
        return result;
    }
}

using System;
using System.Collections.Generic;

public class SlotCastPatternProcessor<T>(List<int> indices) : IPatternProcessor<List<T>> {
    readonly List<int> indices = indices;

    public List<T> Process(List<IToken> tokens, int start, int end) {
        return indices.Select(index => (T)tokens[index]).ToList();
    }
}

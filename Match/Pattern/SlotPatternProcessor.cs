using System;
using System.Collections.Generic;

public class SlotPatternProcessor : IPatternProcessor<List<Token>> {
    List<int> indices;
    
    public SlotPatternProcessor(List<int> indices) {
        this.indices = indices;
    }

    public List<Token> Process(List<Token> tokens, int start, int end) {
        List<Token> result = new List<Token>();
        foreach (int index in indices) {
            result.Add(tokens[index]);
        }
        return result;
    }
}

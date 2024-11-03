namespace Epsilon;
public class SlotPatternProcessor(List<int> indices) : IPatternProcessor<List<IToken>> {
    readonly List<int> indices = indices;

    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        return indices.Select(index => tokens[index]).ToList();
    }
}

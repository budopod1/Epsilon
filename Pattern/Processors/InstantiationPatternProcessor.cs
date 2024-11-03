namespace Epsilon;
public class InstantiationPatternProcessor(Type type) : IPatternProcessor<List<IToken>> {
    readonly Type type = type;

    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        IToken result = (IToken)Activator.CreateInstance(type);
        return [result];
    }
}

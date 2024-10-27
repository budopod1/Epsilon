public class WrapperPatternProcessor : IPatternProcessor<List<IToken>> {
    readonly Type wrapper;
    readonly IPatternProcessor<List<IToken>> subprocessor;

    public WrapperPatternProcessor(IPatternProcessor<List<IToken>> subprocessor, Type wrapper) {
        this.wrapper = wrapper;
        this.subprocessor = subprocessor;
    }

    public WrapperPatternProcessor(Type wrapper) {
        this.wrapper = wrapper;
        subprocessor = null;
    }

    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        List<IToken> ntokens = tokens;
        if (subprocessor != null) {
            ntokens = subprocessor.Process(tokens, start, end);
        }
        IToken result = (IToken)Activator.CreateInstance(
            wrapper, [ntokens]
        );
        return [result];
    }
}

public class FuncTemplate(PatternExtractor<List<IToken>> pattern,
                    List<FunctionArgumentToken> arguments) : Unit<PatternExtractor<List<IToken>>>(pattern),
                            IMultiLineToken, IBarMatchingInto {
    readonly List<FunctionArgumentToken> arguments = arguments;

    public List<FunctionArgumentToken> GetArguments() {
        return arguments;
    }
}

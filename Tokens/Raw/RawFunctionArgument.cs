public class RawFunctionArgument(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return new RawFunctionArgument(tokens);
    }
}

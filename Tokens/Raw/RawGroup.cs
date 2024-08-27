public class RawGroup(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return new RawGroup(tokens);
    }
}

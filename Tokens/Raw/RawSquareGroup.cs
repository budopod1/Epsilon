namespace Epsilon;
public class RawSquareGroup(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return new RawSquareGroup(tokens);
    }
}

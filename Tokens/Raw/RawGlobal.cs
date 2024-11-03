namespace Epsilon;
public class RawGlobal(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return new RawGlobal(tokens);
    }
}

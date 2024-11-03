namespace Epsilon;
public class Block(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return new Block(tokens);
    }
}

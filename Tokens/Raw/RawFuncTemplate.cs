namespace Epsilon;
public class RawFuncTemplate(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return new RawFuncTemplate(tokens);
    }
}

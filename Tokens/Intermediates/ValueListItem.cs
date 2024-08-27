public class ValueListItem(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return new ValueListItem(tokens);
    }
}

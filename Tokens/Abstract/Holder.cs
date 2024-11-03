namespace Epsilon;
public abstract class Holder(List<IToken> tokens) : TreeToken(tokens) {
    public Block GetBlock() {
        if (Count < 2) return null;
        IToken token = this[1];
        if (!(token is Block)) return null;
        return (Block)token;
    }

    public void SetBlock(Block block) {
        if (Count < 2)
            throw new InvalidOperationException("Holder does not have block already set");
        this[1] = block;
    }
}

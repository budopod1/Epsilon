namespace Epsilon;
public class TraverseConfig {
    public TraverseMode Mode;
    public bool Invert;
    public bool YieldFirst;
    public Func<IToken, bool> AvoidTokens;

    public TraverseConfig() {
        Mode = TraverseMode.DEPTH;
        Invert = false;
        YieldFirst = false;
        AvoidTokens = token => false;
    }

    public TraverseConfig(TraverseMode mode, bool invert, bool yieldFirst, Func<IToken, bool> avoidTokens) {
        Mode = mode;
        Invert = invert;
        YieldFirst = yieldFirst;
        AvoidTokens = avoidTokens;
    }
}

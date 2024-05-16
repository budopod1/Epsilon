using System;

public class TraverseConfig {
    public TraverseMode Mode;
    public bool Invert;
    public bool YieldFirst;

    public TraverseConfig() {
        Mode = TraverseMode.DEPTH;
        Invert = false;
    }

    public TraverseConfig(TraverseMode mode, bool invert, bool yieldFirst) {
        Mode = mode;
        Invert = invert;
        YieldFirst = yieldFirst;
    }
}

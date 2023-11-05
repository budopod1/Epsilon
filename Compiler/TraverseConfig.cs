using System;

public class TraverseConfig {
    public TraverseMode Mode;
    public bool Invert;

    public TraverseConfig() {
        Mode = TraverseMode.DEPTH;
        Invert = false;
    }

    public TraverseConfig(TraverseMode mode, bool invert) {
        Mode = mode;
        Invert = invert;
    }
}

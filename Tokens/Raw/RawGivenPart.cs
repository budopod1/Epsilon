using System;

public class RawGivenPart : IParentToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    RawGivenValue rawGivenValue;
    CodeBlock block;
    readonly Type_ toType_;
    readonly int varID;

    public int Count {
        get => 2;
    }

    public IToken this[int i] {
        get {
            if (i == 0) {
                return rawGivenValue;
            } else {
                return block;
            }
        }
        set {
            if (i == 0) {
                rawGivenValue = (RawGivenValue)value;
            } else {
                block = (CodeBlock)value;
            }
        }
    }

    public RawGivenPart(RawGivenValue givenValue, VarDeclaration var_, CodeBlock block) {
        rawGivenValue = givenValue;
        this.block = block;
        string varName = var_.GetName().GetValue();
        toType_ = var_.GetType_();
        varID = block.GetScope().AddVar(
            varName, toType_
        );
    }

    public RawGivenValue GetRawValue() {
        return rawGivenValue;
    }

    public CodeBlock GetBlock() {
        return block;
    }

    public Type_ GetToType_() {
        return toType_;
    }

    public int GetVarID() {
        return varID;
    }
}

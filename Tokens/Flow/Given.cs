public class Given : IFlowControl, IFunctionTerminator {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly List<GivenPart> parts;
    CodeBlock else_ = null;

    public int Count {
        get { return parts.Count + (else_==null?0:1); }
    }

    public IToken this[int i] {
        get {
            if (i == parts.Count) return else_;
            return parts[i];
        }
        set {
            if (i == parts.Count) {
                else_ = (CodeBlock)value;
            } else {
                parts[i] = (GivenPart)value;
            }
        }
    }

    public Given(RawGivenPart part) {
        parts = [new(part)];
    }

    public Given(Given given, RawGivenPart part) {
        if (given.GetElse() != null) {
            throw new SyntaxErrorException(
                "Cannot add part to given already terminated with else", part
            );
        }
        parts = new List<GivenPart>(given.GetParts())
        {
            new GivenPart(part)
        };
    }

    public Given(Given given, CodeBlock else_) {
        if (given.GetElse() != null) {
            throw new SyntaxErrorException(
                "Cannot add else to given already terminated with else", else_
            );
        }
        parts = given.GetParts();
        this.else_ = else_;
    }

    public List<GivenPart> GetParts() {
        return parts;
    }

    public CodeBlock GetElse() {
        return else_;
    }

    public int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this) {
            ["parts"] = parts.Select(part => new Dictionary<string, object> {
                {"val", part.GetValue()}, {"to_type_", part.GetToType_()},
                {"var_id", part.GetVarID()}, {"block", part.GetBlock()}
            }),
            ["else"] = else_
        }.Register();
    }

    public bool DoesTerminateFunction() {
        if (else_ == null) return false;
        if (!else_.DoesTerminateFunction()) return false;
        return parts.All(part =>
            part.GetBlock().DoesTerminateFunction());
    }
}

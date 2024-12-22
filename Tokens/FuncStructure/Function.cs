using CsJSONTools;

namespace Epsilon;
public class Function : RealFunctionDeclaration, IParentToken, ITopLevel, IVerifier {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    static readonly List<IPatternSegment> mainPattern = [
        new UnitPatternSegment<string>(typeof(Name), "main")
    ];

    public int Count {
        get { return 1; }
    }

    public IToken this[int i] {
        get {
            return block;
        }
        set {
            block = (CodeBlock)value;
        }
    }

    readonly string sourcePath;
    readonly PatternExtractor<List<IToken>> pattern;
    readonly List<FunctionArgument> arguments;
    CodeBlock block;
    readonly Type_ returnType_;
    readonly bool doesReturnVoid;
    readonly string id;
    readonly string callee;
    readonly List<Type_> specialAllocs = [];

    public Function(string idPath, Program program, PatternExtractor<List<IToken>> pattern, List<FunctionArgumentToken> arguments, CodeBlock block, Type_ returnType_, List<IAnnotation> annotations) {
        sourcePath = idPath;
        this.pattern = pattern;
        this.block = block;
        this.returnType_ = returnType_;
        doesReturnVoid = returnType_ == null;
        IScope scope = block.GetScope();
        foreach (FunctionArgumentToken argument in arguments) {
            argument.SetID(scope.AddVar(
                argument.GetName(), argument.GetType_()
            ));
        }
        this.arguments = arguments.Select(
            argument=>new FunctionArgument(argument)
        ).ToList();
        id = $"{idPath}/{program.GetFunctionID()}";

        bool setCallee = false;
        foreach (IAnnotation annotation in annotations) {
            if (annotation is IDAnnotation idA && !setCallee) {
                callee = idA.GetID();
                setCallee = true;
            }
        }
        if (Enumerable.SequenceEqual(mainPattern, this.pattern.GetSegments()) && !setCallee) {
            callee = "main";
            setCallee = true;
        }
        if (!setCallee) {
            callee = id;
            setCallee = true;
        }
    }

    public override PatternExtractor<List<IToken>> GetPattern() {
        return pattern;
    }

    public override List<FunctionArgument> GetArguments() {
        return arguments;
    }

    public CodeBlock GetBlock() {
        return block;
    }

    public void SetBlock(CodeBlock block) {
        this.block = block;
    }

    protected override Type_ _GetReturnType_() {
        return returnType_;
    }

    public override bool DoesReturnVoid() {
        return doesReturnVoid;
    }

    public override string GetID() {
        return id;
    }

    public override FunctionSource GetSource() {
        return FunctionSource.Program;
    }

    public override string GetCallee() {
        return callee;
    }

    public override string GetSourcePath() {
        return sourcePath;
    }

    public override bool TakesOwnership() {
        return true;
    }

    public override bool ResultInParams() {
        return true;
    }

    public override string ToString() {
        string title = Utils.WrapName(
            GetType().Name, string.Join(", ", arguments), "<", ">"
        );
        return Utils.WrapName(title, block.ToString());
    }

    public JSONObject GetFullJSON() {
        JSONObject obj = GetJSON();
        obj["block"] = SerializationContext.SerializeBlock(null, block);
        obj["vars"] = new JSONList(
            TokenUtils.TraverseFind<CodeBlock>(this).SelectMany(
                block => Scope.GetVarsJSON(block.GetScope())));
        obj["special_allocs"] = new JSONList(
            specialAllocs.Select(type_=>type_.GetJSON())
        );
        obj["is_main"] = new JSONBool(IsMain());
        return obj;
    }

    public void Verify() {
        if (IsMain()) {
            if (doesReturnVoid || !returnType_.Equals(new Type_("Z"))) {
                throw new SyntaxErrorException(
                    "The main function must return type Z", this
                );
            }
        }
        if (!doesReturnVoid) {
            if (block.Count == 0) {
                throw new SyntaxErrorException(
                    "Functions with a non-void return value cannot be empty", block
                );
            }
            if (!block.DoesTerminateFunction()) {
                throw new SyntaxErrorException(
                    "Functions with a non-void return value must end with a function terminator", block
                );
            }
        }
    }

    public int AddSpecialAlloc(Type_ type_) {
        specialAllocs.Add(type_);
        return specialAllocs.Count-1;
    }
}

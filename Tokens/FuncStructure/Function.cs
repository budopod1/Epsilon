using System;
using System.Linq;
using System.Collections.Generic;

public class Function : RealFunctionDeclaration, IParentToken, ITopLevel, IVerifier {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    static List<IPatternSegment> mainPattern = new List<IPatternSegment> {
        new UnitPatternSegment<string>(typeof(Name), "main")
    };
    public Dictionary<ISerializableToken, int> Serialized = new Dictionary<ISerializableToken, int>();

    public int Count {
        get { return 1; }
    }

    public IToken this[int i] {
        get {
            return block;
        }
        set {
            block = ((CodeBlock)value);
        }
    }

    string sourcePath;
    PatternExtractor<List<IToken>> pattern;
    List<FunctionArgument> arguments;
    CodeBlock block;
    Type_ returnType_;
    bool doesReturnVoid;
    List<Type_> specialAllocs = new List<Type_>();
    string id;
    string callee;
    List<SerializationContext> contexts = new List<SerializationContext>();

    public Function(string idPath, Program program, PatternExtractor<List<IToken>> pattern, List<FunctionArgumentToken> arguments, CodeBlock block, Type_ returnType_, List<IAnnotation> annotations) {
        this.sourcePath = idPath;
        this.pattern = pattern;
        this.block = block;
        this.returnType_ = returnType_;
        this.doesReturnVoid = returnType_ == null;
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
            IDAnnotation idA = annotation as IDAnnotation;
            if (idA != null && !setCallee) {
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

    public override string GetCallee()  {
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
            GetType().Name, String.Join(", ", arguments), "<", ">"
        );
        return Utils.WrapName(title, block.ToString());
    }

    public int RegisterContext(SerializationContext context) {
        contexts.Add(context);
        return contexts.Count-1;
    }

    public int? GetContextIdByBlock(CodeBlock block) {
        int i = 0;
        foreach (SerializationContext context in contexts) {
            if (context.GetBlock() == block) {
                return i;
            }
            i++;
        }
        return null;
    }

    public JSONObject GetFullJSON() {
        JSONObject obj = GetJSON();
        new SerializationContext(this).Serialize(block);
        obj["blocks"] = new JSONList(contexts.Select(
            context=>context.Serialize()
        ));
        JSONList declarations = new JSONList();
        foreach (CodeBlock block in TokenUtils.TraverseFind<CodeBlock>(this)) {
            foreach (IJSONValue declaration in Scope.GetVarsJSON(block.GetScope())) {
                declarations.Add(declaration);
            }
        }
        obj["declarations"] = declarations;
        obj["special_allocs"] = new JSONList(
            specialAllocs.Select(specialAlloc=>specialAlloc.GetJSON())
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

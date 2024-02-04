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
    
    PatternExtractor<List<IToken>> pattern;
    List<FunctionArgument> arguments;
    CodeBlock block;
    Type_ returnType_;
    List<Type_> specialAllocs = new List<Type_>();
    string id;
    string callee;
    List<SerializationContext> contexts = new List<SerializationContext>();
    bool isMain;
    
    public Function(Program program, PatternExtractor<List<IToken>> pattern, 
                    List<FunctionArgumentToken> arguments, CodeBlock block,
                    Type_ returnType_) {
        this.pattern = pattern;
        this.block = block;
        this.returnType_ = returnType_;
        isMain = Enumerable.SequenceEqual(mainPattern, this.pattern.GetSegments());
        Scope scope = block.GetScope();
        foreach (FunctionArgumentToken argument in arguments) {
            argument.SetID(scope.AddVar(
                argument.GetName(), argument.GetType_()
            ));
        }
        this.arguments = arguments.Select(
            argument=>new FunctionArgument(argument)
        ).ToList();
        id = program.GetPath() + program.GetFunctionID().ToString();
        callee = isMain ? "main" : id;
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

    public override Type_ GetReturnType_() {
        return returnType_;
    }

    public override Type_ GetReturnType_(List<IValueToken> tokens) {
        return GetReturnType_();
    }

    public override string GetID() {
        return id;
    }

    public override string GetCallee()  {
        return callee;
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

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["id"] = new JSONString(id);
        obj["callee"] = new JSONString(callee);
        obj["arguments"] = new JSONList(arguments.Select(
            argument => argument.GetJSON()
        ));
        obj["return_type_"] = returnType_.GetJSON();
        new SerializationContext(this).Serialize(block);
        obj["blocks"] = new JSONList(contexts.Select(
            context=>context.Serialize()
        ));
        JSONList declarations = new JSONList();
        foreach (CodeBlock block in TokenUtils.TraverseFind<CodeBlock>(this)) {
            foreach (IJSONValue declaration in block.GetScope().GetVarsJSON()) {
                declarations.Add(declaration);
            }
        }
        obj["declarations"] = declarations;
        obj["special_allocs"] = new JSONList(
            specialAllocs.Select(specialAlloc=>specialAlloc.GetJSON())
        );
        obj["is_main"] = new JSONBool(isMain);
        return obj;
    }

    public void Verify() {
        if (isMain) {
            if (!returnType_.Equals(new Type_("Z"))) {
                throw new SyntaxErrorException(
                    "The main function must return type Z", this
                );
            }
        }
        if (!returnType_.GetBaseType_().IsVoid()) {
            if (block.Count == 0) {
                throw new SyntaxErrorException(
                    "Functions with a non-void return value cannot be empty", block
                );
            }
            IToken token = block[block.Count-1];
            Line line = token as Line;
            if (line == null) return;
            line.Verify(); // make sure line.Count == 1
            if (!(line[0] is Return)) {
                throw new SyntaxErrorException(
                    "Functions with a non-void return value must end with a return statement", line
                );
            }
        }
    }

    public bool IsMain() {
        return isMain;
    }

    public int AddSpecialAlloc(Type_ type_) {
        specialAllocs.Add(type_);
        return specialAllocs.Count-1;
    }
}

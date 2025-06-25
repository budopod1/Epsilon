using CsJSONTools;

namespace Epsilon;
public class HeaderFileCompiler : IFileCompiler {
    readonly string path;
    readonly string idPath;
    readonly string fileText;
    readonly bool isCPP;

    string implementation = null;
    readonly Dictionary<string, LocatedID> structIDs = [];
    readonly HashSet<Struct> structs = [];
    readonly List<RealFunctionDeclaration> funcs = [];

    string ir = null;
    string obj = null;

    static readonly string[] headerImplementationExtensions = ["c", "cpp", "cc", "cxx"];
    static readonly string[] headerExtensions = ["h", "hpp", "hxx"];
    static readonly string[] cppExtensions = ["hpp", "hxx", "cpp", "cc", "cxx"];
    static readonly string[] LLVMImplementationExtensions = ["ll", "bc"];

    class LineReader(string source) {
        readonly string[] parts = source.Split("\n");
        int i = 0;

        public string Line() {
            return parts[i++];
        }

        public int Int() {
            return int.Parse(Line());
        }
    }

    public static void Setup() {
        Builder.RegisterDispatcher((BuildSettings buildSettings, string path) => {
            string fileText = JSONTools.ReadFileText(new StreamReader(path));
            string idPath = buildSettings.GetIDPath(path);
            return new HeaderFileCompiler(path, idPath, fileText);
        }, [..headerExtensions, ..headerImplementationExtensions]);
    }

    HeaderFileCompiler(string path, string idPath, string fileText) {
        this.path = path;
        this.idPath = idPath;
        Log.Info("Compiling file from header", idPath);
        this.fileText = fileText;
        string pathExtension = Utils.GetExtension(path);
        isCPP = cppExtensions.Contains(pathExtension);
        if (headerImplementationExtensions.Contains(pathExtension)) {
            implementation = path;
        }
    }

    public string GetText() {
        return fileText;
    }

    public string GetIDPath() {
        return idPath;
    }

    LocatedID ReadStructID(LineReader reader) {
        return new LocatedID(
            idPath, reader.Line()
        );
    }

    Type_ ReadType_(LineReader reader) {
        string baseType_Name = reader.Line();
        if (structIDs.TryGetValue(baseType_Name, out LocatedID value))
            baseType_Name = value.GetID();
        if (baseType_Name == "") return null;
        int? baseType_Bits = reader.Int();
        if (baseType_Bits == -1) baseType_Bits = null;
        int genericCount = reader.Int();
        List<Type_> generics = [];
        for (int i = 0; i < genericCount; i++) {
            generics.Add(ReadType_(reader));
        }
        return new Type_(baseType_Name, baseType_Bits, generics);
    }

    Struct ReadStruct(LineReader reader) {
        string name = reader.Line();

        if (!structIDs.ContainsKey(name)) {
            throw new FileProblemException($"Struct '{name}' exposed in public interface doesn't have an exposed definition");
        }

        int fieldCount = reader.Int();
        List<Field> fields = [];
        for (int i = 0; i < fieldCount; i++) {
            Type_ fieldType_ = ReadType_(reader);
            string fieldName = reader.Line();
            fields.Add(new Field(fieldName, fieldType_));
        }
        string destructorSymbol = reader.Line();
        if (destructorSymbol == "") destructorSymbol = null;
        return new Struct(idPath, name, fields, "structs." + name, destructorSymbol,
            globalFreeFn: false, extendeeID: null);
    }

    RealExternalFunction ReadFunc(LineReader reader) {
        string symbol = reader.Line();
        Type_ retType_ = ReadType_(reader);
        string name = reader.Line();
        int argCount = reader.Int();

        List<FunctionArgument> arguments = [];
        List<IPatternSegment> segments = [];
        List<int> argumentIdxs = [];

        segments.Add(new UnitPatternSegment<string>(typeof(Name), name));

        for (int i = 0; i < argCount; i++) {
            Type_ argType_ = ReadType_(reader);
            string argName = reader.Line();
            arguments.Add(new FunctionArgument(argName, argType_));
            segments.Add(new FuncArgPatternSegment());
            argumentIdxs.Add(i+1);
        }

        return new RealExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                segments, new SlotPatternProcessor(argumentIdxs)
            ), arguments, name, idPath, symbol, retType_, retType_ == null,
            FunctionSource.Program, takesOwnership: false, resultInParams: true
        );
    }

    void ReadSerializedSignatures(LineReader reader) {
        string implementationIn = reader.Line();

        if (implementationIn != "") {
            implementation = Utils.JoinPaths(
                Utils.GetDirectoryName(path),
                implementationIn
            );
        }

        if (implementation == null) {
            throw new FileProblemException(
                "Header files must include #define EPSL_IMPLEMENTATION_LOCATION \"<implementation location>\""
            );
        }

        int structCount = reader.Int();
        for (int i = 0; i < structCount; i++) {
            LocatedID id = ReadStructID(reader);
            structIDs[id.Name] = id;
        }
        for (int i = 0; i < structCount; i++) {
            structs.Add(ReadStruct(reader));
        }

        int funcCount = reader.Int();
        for (int i = 0; i < funcCount; i++) {
            funcs.Add(ReadFunc(reader));
        }
    }

    IEnumerable<string> FetchSignaturesArgs(string filename) {
        return new string[] {filename, isCPP ? "c++" : "c"}
            .Concat(CmdUtils.ListIncludeDirs(isCPP).Select(arg => "-I"+arg))
            .Concat(Subconfigs.GetClangParseConfigs());
    }

    void FetchSignatures(string filename) {
        string executablePath = $"Compilers{Path.DirectorySeparatorChar}signatures";
        string filePath = $"temp{Path.DirectorySeparatorChar}{filename}";
        string executableResult = CmdUtils.RunProjExecutable(
            executablePath, FetchSignaturesArgs(filePath));

        LineReader reader = new(executableResult);
        string status = reader.Line();
        switch (status) {
        case "success":
            ReadSerializedSignatures(reader);
            break;
        case "input error":
            throw new SyntaxErrorException(reader.Line(), new CodeSpan(
                reader.Int(), reader.Int()
            ));
        case "processing error":
            throw new FileProblemException(reader.Line());
        default:
            throw new InvalidOperationException($"Got status '{status}'");
        }
    }

    public IEnumerable<string> ToImports() {
        return [];
    }

    public SubconfigCollection GetSubconfigs() {
        List<ISubconfig> linkingSubconfigs = [];
        if (isCPP) {
            linkingSubconfigs.Add(new ConstantSubconfig(["-lstdc++"]));
        }
        return new SubconfigCollection([], linkingSubconfigs, []);
    }

    public HashSet<LocatedID> ToStructIDs() {
        string destName = Utils.GetFileName(path);
        string errors = CmdUtils.VerifyCSyntax(isCPP, path);
        if (errors.Length > 0) {
            throw new FileProblemException(errors);
        }
        File.Copy(path, Utils.JoinPaths(Utils.TempDir(), destName), true);
        FetchSignatures(destName);

        return [..structIDs.Values];
    }

    public void AddStructIDs(HashSet<LocatedID> structIds) {}

    public List<RealFunctionDeclaration> ToDeclarations() {
        return funcs;
    }

    public void AddDeclarations(List<RealFunctionDeclaration> declarations) {}

    public HashSet<Struct> ToStructs() {
        return structs;
    }

    public void LoadStructExtendees() {}

    public Dependencies ToDependencies(Builder builder) {
        return Dependencies.Empty();
    }

    bool IsSelfImplementing() {
        return headerImplementationExtensions.Contains(
            Utils.GetExtension(implementation));
    }

    public void FinishCompilation(string destPath, bool recommendLLVM) {
        string implementationExtension = Utils.GetExtension(implementation);
        if (IsSelfImplementing()) {
            ir = destPath + ".bc";
            CmdUtils.CToLLVM(isCPP, implementation, ir);
        } else if (LLVMImplementationExtensions.Contains(implementationExtension)) {
            ir = destPath + "." + implementationExtension;
            File.Copy(implementation, ir, overwrite: true);
        } else if (implementationExtension == "o") {
            obj = destPath + ".o";
            File.Copy(implementation, obj, overwrite: true);
        } else {
            throw new FileProblemException(
                $".{implementationExtension} is not a valid extension for a header's implementation"
            );
        }
    }

    public string GetIR() {
        return ir;
    }

    public string GetObj() {
        return obj;
    }

    public bool FromCache() {
        return false;
    }

    public bool ShouldSaveSPEC() {
        return false;
    }

    public FileSourceType GetFileSourceType() {
        return FileSourceType.User;
    }
}

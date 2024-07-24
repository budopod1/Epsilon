using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class CFileCompiler : IFileCompiler {
    string path;
    string idPath;
    string fileText;
    bool isCPP;

    Dictionary<string, LocatedID> structIDs = new Dictionary<string, LocatedID>();
    HashSet<Struct> structs = new HashSet<Struct>();
    List<RealFunctionDeclaration> funcs = new List<RealFunctionDeclaration>();

    string ir = null;
    string obj = null;

    static string[] cExtensions = new string[] {"c"};
    static string[] cppExtensions = new string[] {"cpp", "cc", "cxx"};

    class LineReader {
        string[] parts;
        int i = 0;

        public LineReader(string source) {
            parts = source.Split("\n");
        }

        public string Line() {
            return parts[i++];
        }

        public int Int() {
            return Int32.Parse(Line());
        }
    }

    public static void Setup() {
        Builder.RegisterDispatcher((BuildSettings settings, string path) => {
            string fileText;
            using (StreamReader file = new StreamReader(path)) {
                fileText = file.ReadToEnd();
            }
            return new CFileCompiler(path, fileText);
        }, cExtensions.Concat(cppExtensions).ToArray());
    }

    CFileCompiler(string path, string fileText) {
        this.path = path;
        idPath = Utils.Stem(path);
        Log.Info("Compiling C file", idPath);
        this.fileText = fileText;
        isCPP = cppExtensions.Contains(Path.GetExtension(path).Substring(1));
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
        if (structIDs.ContainsKey(baseType_Name))
            baseType_Name = structIDs[baseType_Name].GetID();
        if (baseType_Name == "") return null;
        int? baseType_Bits = reader.Int();
        if (baseType_Bits == -1) baseType_Bits = null;
        int genericCount = reader.Int();
        List<Type_> generics = new List<Type_>();
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
        List<Field> fields = new List<Field>();
        for (int i = 0; i < fieldCount; i++) {
            Type_ fieldType_ = ReadType_(reader);
            string fieldName = reader.Line();
            fields.Add(new Field(fieldName, fieldType_));
        }
        return new Struct(idPath, name, fields, "structs."+name, null);
    }

    RealFunctionDeclaration ReadFunc(LineReader reader) {
        string symbol = reader.Line();
        Type_ retType_ = ReadType_(reader);
        string name = reader.Line();
        int argCount = reader.Int();

        List<FunctionArgument> arguments = new List<FunctionArgument>();
        List<IPatternSegment> segments = new List<IPatternSegment>();
        List<int> argumentIdxs = new List<int>();

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
        return new string[] {filename}.Concat(
            CmdUtils.ListIncludeDirs(isCPP).Select(arg => "-I"+arg));
    }

    void FetchSignatures(string filename) {
        string scriptPath = $"scripts{Path.DirectorySeparatorChar}fetchsignatures.bash";
        LineReader reader = new LineReader(CmdUtils.RunScript(
            scriptPath, FetchSignaturesArgs(filename)));
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
        return new string[0];
    }

    public HashSet<LocatedID> ToStructIDs() {
        string destName = Utils.GetFileName(path);
        string errors = CmdUtils.VerifyCSyntax(isCPP, path, new string[0]);
        if (errors.Length > 0) {
            throw new FileProblemException(errors);
        }
        File.Copy(path, Utils.JoinPaths(Utils.TempDir(), destName), true);
        FetchSignatures(destName);

        return structIDs.Values.ToHashSet();
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

    public void FinishCompilation(string destPath, bool recommendLLVM) {
        if (recommendLLVM) {
            ir = destPath + ".bc";
            CmdUtils.CToLLVM(isCPP, path, ir, new string[0]);
        } else {
            obj = destPath + ".o";
            CmdUtils.CToObj(isCPP, path, obj, new string[0]);
        }
    }

    public string GetIR() {
        return ir;
    }

    public string GetObj() {
        return obj;
    }

    public string GetSource() {
        return idPath;
    }

    public bool FromCache() {
        return false;
    }

    public bool ShouldSaveSPEC() {
        return true;
    }

    public IEnumerable<IClangConfig> GetClangConfig() {
        return new List<IClangConfig>();
    }

    public FileSourceType GetFileSourceType() {
        return FileSourceType.User;
    }
}

using System;
using System.Linq;
using System.Collections.Generic;

public class FileTree {
    public string PartialPath;
    public IFileCompiler Compiler;
    public SPECFileCompiler OldCompiler;
    public string GeneratedEPSLSPEC = null;
    public List<FileTree> Imported = new List<FileTree>();
    public List<string> Imports;
    public bool TreeLoaded = false;

    string _Text;
    public string Text { 
        get {
            if (_Text == null) _Text = Compiler.GetText();
            return _Text;
        } 
        set {
            _Text = value;
        }
    }

    string _Path;
    public string Stemmed;
    public string Path { 
        get {
            return _Path;
        } 
        set {
            _Path = value;
            Stemmed = Utils.Stem(value);
        }
    }

    public string OldText {
        get {
            return OldCompiler.GetText();
        }
    }

    public string OldPath;

    public HashSet<LocatedID> StructIDs;
    public HashSet<Struct> Structs;
    public List<RealFunctionDeclaration> Declarations;
    public Dependencies Dependencies;

    public HashSet<Struct> OldStructs;
    public List<RealFunctionDeclaration> OldDeclarations;

    public string IR;

    public FileTree(string partialPath, string path, IFileCompiler compiler, string oldCompilerPath, SPECFileCompiler oldCompiler, string generatedEPSLSPEC) {
        PartialPath = partialPath;
        Path = path;
        Compiler = compiler;
        OldPath = oldCompilerPath;
        OldCompiler = oldCompiler;
        Imports = compiler.ToImports();
        GeneratedEPSLSPEC = generatedEPSLSPEC;
    }

    public JSONObject ToSPEC(Func<string, FileTree> getFile) {
        SPECType_Creator type_Creator = new SPECType_Creator();

        JSONObject obj = new JSONObject();

        obj["functions"] = new JSONList(Declarations.Select(declaration => {
            JSONObject dobj = new JSONObject();
            dobj["id"] = new JSONString(declaration.GetID());
            dobj["callee"] = new JSONString(declaration.GetCallee());
            string returnType_ = type_Creator.MakeSPECType_(declaration.GetReturnType_());
            dobj["return_type_"] = new JSONString(returnType_);
            List<IPatternSegment> segments = declaration.GetPattern().GetSegments();
            List<FunctionArgument> arguments = declaration.GetArguments();
            int argumentCounter = 0;
            dobj["template"] = new JSONList(segments.Select(segment => {
                JSONObject sobj = new JSONObject();
                if (segment is UnitPatternSegment<string>) {
                    sobj["type"] = new JSONString("name");
                    string name = ((UnitPatternSegment<string>)segment).GetValue();
                    sobj["name"] = new JSONString(name);
                } else if (segment is TextPatternSegment) {
                    sobj["type"] = new JSONString("text");
                    string text = ((TextPatternSegment)segment).GetText();
                    sobj["text"] = new JSONString(text);
                } else if (segment is TypePatternSegment) {
                    sobj["type"] = new JSONString("argument");
                    FunctionArgument argument = arguments[argumentCounter++];
                    sobj["name"] = new JSONString(argument.GetName());
                    string type_ = type_Creator.MakeSPECType_(argument.GetType_());
                    sobj["type_"] = new JSONString(type_);
                } else {
                    throw new InvalidOperationException();
                }
                return sobj;
            }));
            dobj["takes_ownership"] = new JSONBool(declaration.TakesOwnership());
            dobj["result_in_params"] = new JSONBool(declaration.ResultInParams());
            dobj["source"] = new JSONString(declaration.GetSource().ToString());
            return dobj;
        }));

        obj["structs"] = new JSONList(Structs.Select(struct_ => {
            JSONObject sobj = new JSONObject();
            sobj["name"] = new JSONString(struct_.GetName());
            sobj["fields"] = new JSONList(struct_.GetFields().Select(field => {
                JSONObject fobj = new JSONObject();
                fobj["name"] = new JSONString(field.GetName());
                string type_ = type_Creator.MakeSPECType_(field.GetType_());
                fobj["type_"] = new JSONString(type_);
                return fobj;
            }));
            return sobj;
        }));

        obj["types_"] = type_Creator.GetJSON();

        obj["dependencies"] = new JSONList(
            Dependencies.GetStructs().GroupBy(struct_ => struct_.GetPath()).ToDictionary(
                group => group.Key, group => group.ToList()
            ).MergeToPairs(
                Dependencies.GetFunctions().GroupBy(func => func.GetSourcePath()).ToDictionary(
                    group => group.Key, group => group.ToList()
                ), () => new List<Struct>(), () => new List<RealFunctionDeclaration>()
            ).Select((KeyValuePair<string, (List<Struct> structs, List<RealFunctionDeclaration> functions)> kvpair) => {
                FileTree file = getFile(kvpair.Key);
                JSONObject dobj = new JSONObject();
                dobj["path"] = new JSONString(kvpair.Key);
                dobj["functions"] = new JSONList(kvpair.Value.functions.Select(
                    func => new JSONInt(file.Declarations.IndexOf(func))
                ));
                dobj["structs"] = new JSONList(kvpair.Value.structs.Select(
                    struct_ => new JSONString(struct_.GetName())
                ));
                return dobj;
            })
        );

        obj["clang_config"] = new JSONList(Compiler.GetClangConfig().Select(
            item => item.GetJSON()));

        obj["imports"] = new JSONList(Imports.Select(import => new JSONString(import)));

        obj["ir"] = new JSONString(IR);

        obj["source"] = new JSONString(Path);

        obj["source_type"] = new JSONString(Compiler.GetFileSourceType().ToString());

        return obj;
    }

    public string GetName() {
        string name = Utils.GetFileNameWithoutExtension(Path);
        if (name[0] == '.') return name.Substring(1);
        return name;
    }
}

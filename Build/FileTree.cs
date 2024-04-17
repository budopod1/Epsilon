using System;
using System.Linq;
using System.Collections.Generic;

public class FileTree {
    public string File;
    public IFileCompiler Compiler;
    public string GeneratedEPSLSPEC = null;
    public List<FileTree> Dependencies = new List<FileTree>();
    public List<string> Imports;
    public bool TreeLoaded = false;

    string _Text;
    public string Text { get {
        if (_Text == null) _Text = Compiler.GetText();
        return _Text;
    } }

    public HashSet<LocatedID> StructIDs;
    public List<Struct> Structs;
    public List<RealFunctionDeclaration> Declarations;

    public string IR;

    public FileTree(string file, IFileCompiler compiler, string generatedEPSLSPEC) {
        File = file;
        Compiler = compiler;
        Imports = compiler.ToImports();
        GeneratedEPSLSPEC = generatedEPSLSPEC;
    }

    public JSONObject ToSPEC() {
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

        obj["clang_config"] = new JSONList(Compiler.GetClangConfig().Select(
            item => item.GetJSON()));

        obj["imports"] = new JSONList(Imports.Select(import => new JSONString(import)));

        obj["ir"] = new JSONString(IR);

        obj["source"] = new JSONString(Compiler.GetSource());

        obj["source_type"] = new JSONString(Compiler.GetFileSourceType().ToString());

        return obj;
    }

    public string GetName() {
        string name = Utils.GetFileNameWithoutExtension(File);
        if (name[0] == '.') return name.Substring(1);
        return name;
    }
}

using System;
using System.Linq;
using System.Collections.Generic;

public class EPSLSPEC {
    public IEnumerable<RealFunctionDeclaration> Functions;
    public IEnumerable<Struct> Structs;
    public Dependencies Dependencies;
    public IEnumerable<IClangConfig> ClangConfig;
    public IEnumerable<string> Imports;
    public string IR;
    public string Source;
    public FileSourceType SourceType;

    public EPSLSPEC(IEnumerable<RealFunctionDeclaration> functions,
        IEnumerable<Struct> structs,
        Dependencies dependencies,
        IEnumerable<IClangConfig> clangConfig,
        IEnumerable<string> imports,
        string IR,
        string source,
        FileSourceType sourceType) {
        Functions = functions;
        Structs = structs;
        Dependencies = dependencies;
        ClangConfig = clangConfig;
        Imports = imports;
        this.IR = IR;
        Source = source;
        SourceType = sourceType;
    }

    public JSONObject ToJSON(Builder builder) {
        SPECType_Creator type_Creator = new SPECType_Creator();

        JSONObject obj = new JSONObject();

        obj["functions"] = new JSONList(Functions.Select(function => {
            JSONObject dobj = new JSONObject();
            dobj["id"] = new JSONString(function.GetID());
            dobj["callee"] = new JSONString(function.GetCallee());
            if (function.DoesReturnVoid()) {
                dobj["return_type_"] = new JSONNull();
            } else {
                string returnType_ = type_Creator.MakeSPECType_(function.GetReturnType_());
                dobj["return_type_"] = new JSONString(returnType_);
            }
            List<IPatternSegment> segments = function.GetPattern().GetSegments();
            List<FunctionArgument> arguments = function.GetArguments();
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
                    // We don't need to save whether the argument uses exactType_Match, because only builtins should
                } else {
                    throw new InvalidOperationException();
                }
                return sobj;
            }));
            dobj["takes_ownership"] = new JSONBool(function.TakesOwnership());
            dobj["result_in_params"] = new JSONBool(function.ResultInParams());
            dobj["source"] = new JSONString(function.GetSource().ToString());
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
                FileTree file = builder.GetFile(kvpair.Key);
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

        obj["clang_config"] = new JSONList(ClangConfig.Select(
            item => item.GetJSON()));

        obj["imports"] = new JSONList(Imports.Select(import => new JSONString(import)));

        obj["ir"] = new JSONString(IR);

        obj["source"] = JSONString.OrNull(Source);

        obj["source_type"] = new JSONString(SourceType.ToString());

        return obj;
    }
}
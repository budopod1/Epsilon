using CsJSONTools;

namespace Epsilon;
public class EPSLSPEC(IEnumerable<RealFunctionDeclaration> functions, IEnumerable<Struct> structs, Dependencies dependencies, SubconfigCollection subconfigs, IEnumerable<string> imports, string IR, string obj, string source, FileSourceType sourceType, string idPath) {
    public IEnumerable<RealFunctionDeclaration> Functions = functions;
    public IEnumerable<Struct> Structs = structs;
    public Dependencies Dependencies = dependencies;
    public SubconfigCollection Subconfigs = subconfigs;
    public IEnumerable<string> Imports = imports;
    public string IR = IR;
    public string Obj = obj;
    public string Source = source;
    public FileSourceType SourceType = sourceType;
    public string IDPath = idPath;

    public static IJSONShape Shape { get => _Shape; }
    static readonly IJSONShape _Shape;

    static EPSLSPEC() {
        _Shape = new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"functions", new JSONListShape(new JSONObjectShape(
                new Dictionary<string, IJSONShape> {
                    {"id", new JSONStringShape()},
                    {"callee", new JSONStringShape()},
                    {"return_type_", new JSONNullableShape(new JSONStringShape())},
                    {"template", new JSONListShape(new JSONObjectShape(
                        new Dictionary<string, IJSONShape> {
                            {"type", new JSONStringShape()}
                        }
                    ))},
                    {"takes_ownership", new JSONBoolShape()},
                    {"result_in_params", new JSONBoolShape()},
                    {"source", new JSONStringOptionsShape([
                        "Program", "Library", "Builtin"
                    ])}
                }
            ))},
            {"types_", new JSONListShape(new JSONObjectShape(new Dictionary<string, IJSONShape> {
                {"given_name", new JSONStringShape()},
                {"name", new JSONStringShape()},
                {"bits", new JSONNullableShape(new JSONIntShape())},
                {"generics", new JSONListShape(new JSONStringShape())}
            }))},
            {"structs", new JSONListShape(new JSONObjectShape(
                new Dictionary<string, IJSONShape> {
                    {"name", new JSONStringShape()},
                    {"fields", new JSONListShape(new JSONObjectShape(
                        new Dictionary<string, IJSONShape> {
                            {"name", new JSONStringShape()},
                            {"type_", new JSONStringShape()}
                        }
                    ))},
                    {"symbol", new JSONStringShape()},
                    {"destructor", new JSONNullableShape(new JSONStringShape())},
                    {"extendee", new JSONNullableShape(new JSONStringShape())},
                    {"global_free_fn", new JSONBoolShape()}
                }
            ))},
            {"dependencies", new JSONListShape(new JSONObjectShape(new Dictionary<string, IJSONShape> {
                {"path", new JSONStringShape()},
                {"functions", new JSONListShape(new JSONWholeShape())},
                {"structs", new JSONListShape(new JSONStringShape())}
            }))},
            {"clang_parse_subconfigs", new JSONListShape(new JSONObjectShape(
                new Dictionary<string, IJSONShape> {{"type", new JSONStringShape()}}
            ))},
            {"linking_configs", new JSONListShape(new JSONObjectShape(
                new Dictionary<string, IJSONShape> {{"type", new JSONStringShape()}}
            ))},
            {"object_gen_configs", new JSONListShape(new JSONObjectShape(
                new Dictionary<string, IJSONShape> {{"type", new JSONStringShape()}}
            ))},
            {"imports", new JSONListShape(new JSONStringShape())},
            {"ir", new JSONNullableShape(new JSONStringShape())},
            {"obj", new JSONNullableShape(new JSONStringShape())},
            {"source", new JSONNullableShape(new JSONStringShape())},
            {"source_type", new JSONStringOptionsShape(["Library", "User"])},
            {"id_path", new JSONStringShape()}
        });
    }

    public JSONObject ToJSON(Builder builder) {
        SPECType_Creator type_Creator = new();

        return new JSONObject {
            ["functions"] = new JSONList(Functions.Select(function => {
                JSONObject dobj = [];
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
                    JSONObject sobj = [];
                    if (segment is UnitPatternSegment<string> segment1) {
                        sobj["type"] = new JSONString("name");
                        string name = segment1.GetValue();
                        sobj["name"] = new JSONString(name);
                    } else if (segment is TextPatternSegment segment2) {
                        sobj["type"] = new JSONString("text");
                        string text = segment2.GetText();
                        sobj["text"] = new JSONString(text);
                    } else if (segment is FuncArgPatternSegment) {
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
            })),

            ["structs"] = new JSONList(Structs.Select(struct_ => new JSONObject {
                ["name"] = new JSONString(struct_.GetName()),
                ["fields"] = new JSONList(struct_.GetFields().Select(field => new JSONObject {
                    ["name"] = new JSONString(field.GetName()),
                    ["type_"] = new JSONString(type_Creator.MakeSPECType_(field.GetType_()))
                })),
                ["symbol"] = new JSONString(struct_.GetSymbol()),
                ["destructor"] = JSONString.OrNull(struct_.GetDestructorSymbol()),
                ["extendee"] = JSONString.OrNull(struct_.GetExtendeeID()),
                ["global_free_fn"] = new JSONBool(struct_.HasGlobalFreeFn())
            })),

            ["types_"] = type_Creator.GetJSON(),

            ["dependencies"] = new JSONList(
                Dependencies.GetStructs().GroupBy(struct_ => struct_.GetPath()).ToDictionary(
                    group => group.Key, group => group.ToList()
                ).MergeToPairs(
                    Dependencies.GetFunctions().GroupBy(func => func.GetSourcePath()).ToDictionary(
                        group => group.Key, group => group.ToList()
                    ), () => [], () => []
                ).Select((KeyValuePair<string, (List<Struct> structs, List<RealFunctionDeclaration> functions)> kvpair) => {
                    FileTree file = builder.GetFileByIDPath(kvpair.Key);
                    return new JSONObject {
                        ["path"] = new JSONString(kvpair.Key),
                        ["functions"] = new JSONList(kvpair.Value.functions.Select(
                            func => new JSONInt(file.Declarations.IndexOf(func))
                        )),
                        ["structs"] = new JSONList(kvpair.Value.structs.Select(
                            struct_ => new JSONString(struct_.GetName())
                        ))
                    };
                })
            ),

            ["clang_parse_subconfigs"] = new JSONList(Subconfigs.ClangParseConfigs.Select(item => item.GetJSON())),

            ["linking_configs"] = new JSONList(Subconfigs.LinkingConfigs.Select(item => item.GetJSON())),

            ["object_gen_configs"] = new JSONList(Subconfigs.ObjectGenConfigs.Select(item => item.GetJSON())),

            ["imports"] = new JSONList(Imports.Select(import => new JSONString(import))),

            ["ir"] = JSONString.OrNull(IR),

            ["obj"] = JSONString.OrNull(Obj),

            ["source"] = JSONString.OrNull(Source),

            ["source_type"] = new JSONString(SourceType.ToString()),

            ["id_path"] = new JSONString(IDPath)
        };
    }
}

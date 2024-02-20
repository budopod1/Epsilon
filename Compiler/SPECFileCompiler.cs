using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class SPECFileCompiler : IFileCompiler {
    string path;
    ShapedJSON obj;
    string fileText = null;
    
    public SPECFileCompiler(string path) {
        this.path = path;
        using (StreamReader file = new StreamReader(path)) {
            fileText = file.ReadToEnd();
        }
    }

    public string GetText() {
        return fileText;
    }

    public List<string> ToImports() {
        IJSONValue jsonValue = JSONTools.ParseJSON(fileText);
        IJSONShape shape = new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"functions", new JSONListShape(new JSONObjectShape(
                new Dictionary<string, IJSONShape> {
                    {"id", new JSONStringShape()},
                    {"callee", new JSONStringShape()},
                    {"return_type_", new JSONStringShape()},
                    {"template", new JSONListShape(new JSONObjectShape(
                        new Dictionary<string, IJSONShape> {
                            {"type", new JSONStringShape()}
                        }
                    ))},
                    {"takes_ownership", new JSONBoolShape()},
                    {"result_in_params", new JSONBoolShape()}
                }
            ))},
            {"structs", new JSONListShape(new JSONObjectShape(
                new Dictionary<string, IJSONShape> {
                    {"name", new JSONStringShape()},
                    {"fields", new JSONListShape(new JSONObjectShape(
                        new Dictionary<string, IJSONShape> {
                            {"name", new JSONStringShape()},
                            {"type_", new JSONStringShape()}
                        }
                    ))}
                }
            ))},
            {"ir", new JSONStringShape()}
        });
        obj = new ShapedJSON(jsonValue, shape);
        return new List<string>();
    }

    IEnumerable<string> GetStructIDs() {
        return obj["structs"].IterList().Select(
            val => val["name"].GetString() + " " + path
        );
    }

    public HashSet<string> ToStructIDs() {
        return new HashSet<string>(GetStructIDs());
    }

    public void AddStructIDs(HashSet<string> structIds) {}

    Type_ MakeSPECType_(string str) {
        /*
        Valid usage:
        Q64 -> Q64
        Str -> Array<Byte>
        Array<Z32> -> Array<Z32>
        Foo<Bar,Baz> -> Foo<Bar, Baz>
        Foo<Bar<Baz>Zoo,Pie> -> Foo<Bar<Baz>, Zoo, Pie>
        Foo<Bar<Baz>___> -> Foo<Bar<Baz>>
        */
        // TODO: add validation
        // TODO: improve greatly
        if (Utils.NameChars.Contains(str[str.Length-1])) str += ".";
        List<UserBaseType_> userBaseTypes_ = new List<UserBaseType_>();
        List<char> seperators = new List<char> {' '};
        string soFar = "";
        for (int i = 0; i < str.Length; i++) {
            char chr = str[i];
            if (chr == ' ') continue;
            if (Utils.NameChars.Contains(chr)) {
                soFar += chr;
            } else {
                if (soFar == "___") {
                    userBaseTypes_.Add(null);
                } else {
                    userBaseTypes_.Add(UserBaseType_.ParseString(
                        soFar, new List<string>(GetStructIDs())
                    ));
                }
                seperators.Add(chr);
            }
        }
        int count = userBaseTypes_.Count;
        List<Type_> types_ = new List<Type_>(new Type_[count]);
        while (types_[0] == null) {
            for (int i = 0; i < count; i++) {
                if (types_[i] == null) {
                    UserBaseType_ userBaseType_ = userBaseTypes_[i];
                    if (userBaseType_ == null) continue;
                    List<Type_> generics = new List<Type_>();
                    int indent = 0;
                    bool success = true;
                    for (int j = i+1; j < count; j++) {
                        char seperator = seperators[j];
                        if (seperator == '<') indent++;
                        if (seperator == '>') indent--;
                        if (indent == 0) break;
                        if (indent == 1) {
                            if (userBaseTypes_[j] == null) continue;
                            Type_ type_ = types_[j];
                            if (type_ == null) {
                                success = false;
                                break;
                            }
                            generics.Add(type_);
                        }
                    }
                    if (success) {
                        types_[i] = userBaseType_.ToType_(generics);
                    }
                }
            }
        }
        return types_[0];
    }

    public List<Struct> ToStructs() {
        return obj["structs"].IterList().Select(
            sobj => new Struct(
                path, sobj["name"].GetString(),
                sobj["fields"].IterList().Select(
                    fobj => new Field(
                        fobj["name"].GetString(), 
                        MakeSPECType_(fobj["type_"].GetString())
                    )
                ).ToList()
            )
        ).ToList();
    }

    public void AddStructs(List<Struct> structs) {}

    public List<RealFunctionDeclaration> ToDeclarations() {
        return obj["functions"].IterList().Select(func => {
            List<FunctionArgument> arguments = new List<FunctionArgument>();
            List<IPatternSegment> segments = new List<IPatternSegment>();
            List<int> argumentIdxs = new List<int>();
            ShapedJSON template = func["template"];
            for (int i = 0; i < template.ListCount(); i++) {
                ShapedJSON sobj = template[i];
                string type = sobj["type"].GetString();
                Dictionary<string, IJSONShape> fields;
                switch (type) {
                    case "name":
                        fields = new Dictionary<string, IJSONShape> {
                            {"name", new JSONStringShape()}
                        };
                        break;
                    case "text":
                        fields = new Dictionary<string, IJSONShape> {
                            {"text", new JSONStringShape()}
                        };
                        break;
                    case "argument":
                        fields = new Dictionary<string, IJSONShape> {
                            {"name", new JSONStringShape()},
                            {"type_", new JSONStringShape()}
                        };
                        break;
                    default:
                        throw new InvalidJSONException(
                            "Invalid type of template segment", 
                            sobj.GetJSON()
                        );
                }

                sobj = sobj.ToShape(new JSONObjectShape(fields));

                switch (type) {
                    case "name":
                        string name1 = sobj["name"].GetString();
                        segments.Add(new UnitPatternSegment<string>(typeof(Name), name1));
                        break;
                    case "text":
                        string text = sobj["text"].GetString();
                        segments.Add(new TextPatternSegment(text));
                        break;
                    case "argument":
                        string name2 = sobj["name"].GetString();
                        Type_ type_ = MakeSPECType_(sobj["type_"].GetString());
                        FunctionArgument argument = new FunctionArgument(name2, type_);
                        arguments.Add(argument);
                        segments.Add(new TypePatternSegment(typeof(RawSquareGroup)));
                        argumentIdxs.Add(i);
                        break;
                }
            }
            string id = func["id"].GetString();
            string callee = func["callee"].GetString();
            bool takesOwnership = func["takes_ownership"].GetBool().Value;
            bool resultInParams = func["result_in_params"].GetBool().Value;
            Type_ returnType_ = MakeSPECType_(func["return_type_"].GetString());
            return (RealFunctionDeclaration)new RealExternalFunction(
                new ConfigurablePatternExtractor<List<IToken>>(
                    segments, new SlotPatternProcessor(argumentIdxs)
                ), arguments, id, callee, returnType_, takesOwnership,
                resultInParams
            );
        }).ToList();
    }

    public void AddDeclarations(List<RealFunctionDeclaration> declarations) {}

    public string ToExecutable(string path) {
        string ir = obj["ir"].GetString();
        File.Copy(Path.Combine(Utils.ProjectAbsolutePath(), "libs", ir), path+".bc", true);
        return path+".bc";
    }
}

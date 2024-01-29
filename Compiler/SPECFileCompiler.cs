using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class SPECFileCompiler : IFileCompiler {
    SPECObj obj;
    string fileText = null;
    
    public SPECFileCompiler(string path) {
        using (StreamReader file = new StreamReader(path)) {
            fileText = file.ReadToEnd();
        }
        SPECParser parser = new SPECParser();
        obj = parser.Parse(fileText);
        ISPECShape shape = new SPECObjShape(new Dictionary<string, ISPECShape> {
            {"functions", new SPECListShape(new SPECObjShape(
                new Dictionary<string, ISPECShape> {
                    {"id", new SPECStrShape()},
                    {"return_type_", new SPECStrShape()},
                    {"template", new SPECListShape(new SPECObjShape(
                        new Dictionary<string, ISPECShape> {
                            {"type", new SPECStrShape()}
                        }
                    ))}
                }
            ))},
            {"structs", new SPECListShape(new SPECObjShape(
                new Dictionary<string, ISPECShape> {
                    {"name", new SPECStrShape()},
                    {"fields", new SPECListShape(new SPECObjShape(
                        new Dictionary<string, ISPECShape> {
                            {"name", new SPECStrShape()},
                            {"type_", new SPECStrShape()}
                        }
                    ))}
                }
            ))},
            {"ir", new SPECStrShape()}
        });
        if (!shape.Matches(obj)) {
            throw new SPECShapeErrorException("File does not match required shape");
        }
    }

    public string GetText() {
        return fileText;
    }

    public List<string> ToImports() {
        return new List<string>();
    }

    List<string> GetStructTypes_() {
        return ((SPECList)obj["structs"]).Select(
            val => ((SPECStr)((SPECObj)val)["name"]).Value
        ).ToList();
    }

    public HashSet<string> ToBaseTypes_() {
        List<string> result = GetStructTypes_();
        result.AddRange(BaseType_.BuiltInTypes_);
        return new HashSet<string>(result);
    }

    public void AddBaseTypes_(HashSet<string> baseTypes_) {}

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
                        soFar, GetStructTypes_()
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
        return ((SPECList)obj["structs"]).Select(
            val => {
                SPECObj sobj = ((SPECObj)val);
                return new Struct(
                    ((SPECStr)sobj["name"]).Value, 
                    ((SPECList)sobj["fields"]).Select(
                        fval => {
                            SPECObj fobj = ((SPECObj)fval);
                            return new Field(
                                ((SPECStr)fobj["name"]).Value, 
                                MakeSPECType_(((SPECStr)fobj["type_"]).Value)
                            );
                        }
                    ).ToList()
                );
            }
        ).ToList();
    }

    public void AddStructs(List<Struct> structs) {}

    public List<RealFunctionDeclaration> ToDeclarations() {
        return ((SPECList)obj["functions"]).Select(fval => {
            SPECObj func = ((SPECObj)fval);
            List<FunctionArgument> arguments = new List<FunctionArgument>();
            List<IPatternSegment> segments = new List<IPatternSegment>();
            List<int> argumentIdxs = new List<int>();
            SPECList template = ((SPECList)func["template"]);
            for (int i = 0; i < template.Count; i++) {
                SPECObj sobj = ((SPECObj)template[i]);
                string type = ((SPECStr)sobj["type"]).Value;
                Dictionary<string, ISPECShape> fields;
                switch (type) {
                    case "name":
                        fields = new Dictionary<string, ISPECShape> {
                            {"name", new SPECStrShape()}
                        };
                        break;
                    case "text":
                        fields = new Dictionary<string, ISPECShape> {
                            {"text", new SPECStrShape()}
                        };
                        break;
                    case "argument":
                        fields = new Dictionary<string, ISPECShape> {
                            {"name", new SPECStrShape()},
                            {"type_", new SPECStrShape()}
                        };
                        break;
                    default:
                        throw new SPECShapeErrorException(
                            "Invalid type of template segment"
                        );
                }

                if (!(new SPECObjShape(fields).Matches(sobj))) {
                    throw new SPECShapeErrorException(
                        "Function template segment does not match required shape"
                    );
                }

                switch (type) {
                    case "name":
                        string name1 = ((SPECStr)sobj["name"]).Value;
                        segments.Add(new UnitPatternSegment<string>(typeof(Name), name1));
                        break;
                    case "text":
                        string text = ((SPECStr)sobj["text"]).Value;
                        segments.Add(new TextPatternSegment(text));
                        break;
                    case "argument":
                        string name2 = ((SPECStr)sobj["name"]).Value;
                        Type_ type_ = MakeSPECType_(((SPECStr)sobj["type_"]).Value);
                        FunctionArgument argument = new FunctionArgument(name2, type_);
                        arguments.Add(argument);
                        segments.Add(new TypePatternSegment(typeof(RawSquareGroup)));
                        argumentIdxs.Add(i);
                        break;
                }
            }
            string id = ((SPECStr)func["id"]).Value;
            Type_ returnType_ = MakeSPECType_(((SPECStr)func["return_type_"]).Value);
            return (RealFunctionDeclaration)new RealExternalFunction(
                new ConfigurablePatternExtractor<List<IToken>>(
                    segments, new SlotPatternProcessor(argumentIdxs)
                ), arguments, id, returnType_
            );
        }).ToList();
    }

    public void AddDeclarations(List<RealFunctionDeclaration> declarations) {}

    public string ToExecutable(string path) {
        string ir = ((SPECStr)obj["ir"]).Value;
        File.Copy(Path.Combine(Utils.ProjectAbsolutePath(), "libs", ir), path+".bc", true);
        return path+".bc";
    }
}

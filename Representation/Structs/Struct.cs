using CsJSONTools;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;

namespace Epsilon;
public class Struct : IEquatable<Struct> {
    readonly LocatedID id;
    readonly List<Field> selfFields = null;
    IEnumerable<Field> allFields = null;
    readonly string symbol;
    readonly string destructorSymbol = null;
    bool partiallyLoaded = true;
    bool globalFreeFn;

    readonly bool isSuper = false;
    readonly bool isAbstract = false;
    Struct extendee = null;
    string extendeeID = null;
    readonly string extendeeName = null;
    readonly ExtendsAnnotation extendsAnnotation = null;

    ulong numID = 0;

    public Struct(string path, string name, List<Field> selfFields, List<IAnnotation> annotations) {
        id = new LocatedID(path, name);
        this.selfFields = selfFields;
        symbol = id.GetID();
        bool hasConcreteAnnotation = false;
        foreach (IAnnotation annotation in annotations) {
            if (annotation is IDAnnotation idAnnotation) {
                symbol = idAnnotation.GetID();
            } else if (annotation is SuperAnnotation) {
                isSuper = true;
            } else if (annotation is ConcreteAnnotation) {
                hasConcreteAnnotation = true;
            } else if (annotation is ExtendsAnnotation extendsAnnotation) {
                this.extendsAnnotation = extendsAnnotation;
                extendeeName = extendsAnnotation.GetExtendee();
            }
        }
        isAbstract = isSuper && !hasConcreteAnnotation;
        globalFreeFn = true;
    }

    public Struct(string path, string name, IEnumerable<Field> allFields, string symbol, string destructorSymbol, bool globalFreeFn, bool isSuper, bool isAbstract, string extendeeID) {
        id = new LocatedID(path, name);
        this.allFields = allFields;
        this.symbol = symbol;
        this.destructorSymbol = destructorSymbol;
        this.globalFreeFn = globalFreeFn;
        this.isSuper = isSuper;
        this.isAbstract = isAbstract;
        this.extendeeID = extendeeID;
        partiallyLoaded = false;
    }

    public void LoadExtendee(Program program) {
        partiallyLoaded = false;

        if (extendsAnnotation == null) return;

        string extendeeName = extendsAnnotation.GetExtendee();
        extendeeID = program.GetStructIDs().FirstOrDefault(id => id.Name == extendeeName)?.GetID();

        if (extendeeID == null) {
            throw new SyntaxErrorException(
                $"Cannot find requested struct to extend '{extendeeName}'",
                extendsAnnotation.GetSpan()
            );
        }

        extendee = StructsCtx.GetStructFromID(extendeeID);

        Struct struct_ = extendee;
        do {
            if (struct_ == this) {
                throw new SyntaxErrorException(
                    $"Detected cyclical extend: '{GetID()}' extends itself",
                    extendsAnnotation.GetSpan()
                );
            }

            struct_ = struct_.GetExtendee();
        } while(struct_ != null);
    }

    public string GetPath() {
        return id.Path;
    }

    public string GetName() {
        return id.Name;
    }

    public string GetID() {
        return id.GetID();
    }

    public ulong GetIDNum() {
        if (numID == 0) {
            byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(GetID()));
            numID = BitConverter.ToUInt64(digest, 0);
        }
        return numID;
    }

    public string GetSymbol() {
        return symbol;
    }

    public string GetDestructorSymbol() {
        return destructorSymbol;
    }

    public bool IsSuper() {
        return isSuper;
    }

    public bool IsAbstract() {
        return isAbstract;
    }

    public string GetExtendeeID() {
        return extendeeID;
    }

    public Struct GetExtendee() {
        if (extendee != null) return extendee;
        if (extendeeID != null) return StructsCtx.GetStructFromID(extendeeID);
        return null;
    }

    public IEnumerable<Field> GetFieldsForComp() {
        return selfFields ?? allFields;
    }

    public string GetExtendeeForComp() {
        return extendeeID ?? extendeeName;
    }

    public bool HasGlobalFreeFn() {
        return globalFreeFn;
    }

    public IEnumerable<Field> GetFields() {
        if (partiallyLoaded) {
            throw new InvalidOperationException(
                "Cannot get struct fields until struct is fully loaded"
            );
        }

        if (allFields == null) {
            if (extendee == null) {
                allFields = selfFields;
            } else {
                allFields = extendee.GetFields().Concat(selfFields);
            }
        }
        return allFields;
    }

    public Field GetField(string name) {
        foreach (Field field in GetFields()) {
            if (field.GetName() == name)
                return field;
        }
        return null;
    }

    public IEnumerable<Struct> ExtendList() {
        yield return this;
        Struct extendee = GetExtendee();
        if (extendee != null) {
            foreach (Struct struct_ in extendee.ExtendList()) {
                yield return struct_;
            }
        }
    }

    public override string ToString() {
        string result = $"Name: {GetName()}, Path: {GetPath()}, Symbol: {symbol}, Destructor Symbol: {destructorSymbol}, Extends: {GetExtendeeForComp()}";
        foreach (Field field in GetFieldsForComp()) {
            result += "\n" + field.ToString();
        }
        return Utils.WrapName(
            "Struct", Utils.WrapWithNewlines(Utils.Indent(result))
        );
    }

    public IJSONValue GetJSON() {
        return new JSONObject {
            ["id"] = new JSONString(GetID()),
            ["id_num"] = new JSONString(GetIDNum().ToString("x")),
            ["name"] = new JSONString(GetName()),
            ["fields"] = new JSONList(GetFields().Select(
                field => field.GetJSON()
            )),
            ["symbol"] = new JSONString(symbol),
            ["destructor"] = JSONString.OrNull(destructorSymbol),
            ["is_super"] = new JSONBool(isSuper),
            ["extendees"] = new JSONList(ExtendList().Select(
                struct_ => new JSONString(struct_.GetIDNum().ToString("x"))
            )),
            ["global_free_fn"] = new JSONBool(globalFreeFn)
        };
    }

    public bool Equals(Struct other) {
        if (GetID() != other.GetID()) return false;
        if (GetSymbol() != other.GetSymbol()) return false;
        if (GetDestructorSymbol() != other.GetDestructorSymbol()) return false;
        if (GetExtendeeForComp() != other.GetExtendeeForComp()) return false;
        return GetFieldsForComp().SequenceEqual(other.GetFieldsForComp());
    }
}

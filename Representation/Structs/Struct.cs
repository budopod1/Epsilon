using CsJSONTools;

public class Struct : IEquatable<Struct> {
    readonly LocatedID id;
    readonly List<Field> selfFields = null;
    IEnumerable<Field> allFields = null;
    readonly string symbol;
    readonly bool isSuper = false;
    bool partiallyLoaded = true;

    Struct extendee = null;
    string extendeeID = null;
    readonly string extendeeName = null;
    readonly ExtendsAnnotation extendsAnnotation = null;

    public Struct(string path, string name, List<Field> selfFields, List<IAnnotation> annotations) {
        id = new LocatedID(path, name);
        this.selfFields = selfFields;
        symbol = id.GetID();
        foreach (IAnnotation annotation in annotations) {
            if (annotation is IDAnnotation idA) {
                symbol = idA.GetID();
            } else if (annotation is SuperAnnotation superA) {
                isSuper = true;
            } else if (annotation is ExtendsAnnotation _extendsAnnotation) {
                extendsAnnotation = _extendsAnnotation;
                extendeeName = _extendsAnnotation.GetExtendee();
            }
        }
    }

    public Struct(string path, string name, IEnumerable<Field> allFields, string symbol, string extendeeID) {
        id = new LocatedID(path, name);
        this.allFields = allFields;
        this.symbol = symbol;
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

    public string GetSymbol() {
        return symbol;
    }

    public bool IsSuper() {
        return isSuper;
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
        string result = $"Name: {GetName()}, Path: {GetPath()}, Symbol: {symbol}, Extends: {GetExtendeeForComp()}";
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
            ["name"] = new JSONString(GetName()),
            ["fields"] = new JSONList(GetFields().Select(
                field => field.GetJSON()
            )),
            ["symbol"] = new JSONString(symbol),
            ["extendee"] = JSONString.OrNull(extendeeID)
        };
    }

    public bool Equals(Struct other) {
        if (GetID() != other.GetID()) return false;
        if (GetSymbol() != other.GetSymbol()) return false;
        if (GetExtendeeForComp() != other.GetExtendeeForComp()) return false;
        return GetFieldsForComp().SequenceEqual(other.GetFieldsForComp());
    }
}

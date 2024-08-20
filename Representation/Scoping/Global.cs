using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class Global {
    readonly string name;
    readonly Type_ type_;

    public Global(string name, Type_ type_) {
        this.name = name;
        this.type_ = type_;
    }

    public Global(RawGlobal raw) {
        if (raw.Count != 1) {
            throw new SyntaxErrorException(
                "Invalid syntax in global", raw
            );
        }
        VarDeclaration declaration = raw[0] as VarDeclaration;
        if (declaration == null) {
            throw new SyntaxErrorException(
                "Invalid syntax in global: expected variable declaration", raw
            );
        }
        name = declaration.GetName().GetValue();
        type_ = declaration.GetType_();
        if (type_.GetBaseType_().GetName() != "Optional") {
            throw new SyntaxErrorException(
                "Global variables must be of type Optional", declaration
            );
        }
    }

    public string GetName() {
        return name;
    }

    public Type_ GetType_() {
        return type_;
    }

    public override string ToString() {
        return $"Global({type_}:{name})";
    }
}

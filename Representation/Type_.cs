using System;
using System.Linq;
using System.Collections.Generic;

public class Type_ : IEquatable<Type_> {
    // https://en.wikipedia.org/wiki/
    // Set_(mathematics)#Special_sets_of_numbers_in_mathematics

    public static Type_ Unknown() {
        return new Type_("Unknown");
    }

    public static Type_ Void() {
        return new Type_("Void");
    }
    
    BaseType_ baseType_; // null signifies Any
    List<Type_> generics;

    public static Type_ Any() {
        return new Type_("Any");
    }

    public static Type_ Common(Type_ a, Type_ b) {
        if (a.GetGenerics().Count>0 || b.GetGenerics().Count>0) 
            return Unknown();
        BaseType_ abt = a.GetBaseType_();
        BaseType_ bbt = b.GetBaseType_();
        bool aToB = abt.IsConvertibleTo(bbt);
        bool bToA = bbt.IsConvertibleTo(abt);
        if (aToB && bToA) {
            if (bbt.GetBits() > abt.GetBits()) {
                return b;
            } else {
                return a;
            }
        } else if (aToB) {
            return b;
        } else if (bToA) {
            return a;
        } else {
            return Unknown();
        }
    }

    public static Type_ CommonSpecific(Type_ a, Type_ b,
                                            string name) {
        if (a.GetGenerics().Count>0 || b.GetGenerics().Count>0) 
            return Unknown();
        int? abits = a.GetBaseType_().GetBits();
        int? bbits = b.GetBaseType_().GetBits();
        int? bits = null;
        if (abits==null) {
            bits = bbits;
        } else if (bbits==null) {
            bits = abits;
        } else {
            bits = Math.Max(abits.Value, bbits.Value);
        }
        return new Type_(name, bits);
    }

    public Type_(BaseType_ baseType_, List<Type_> generics = null) {
        this.baseType_ = baseType_;
        if (generics == null) {
            generics = new List<Type_>();
        }
        this.generics = generics;
    }

    public Type_(string name, int? bits, List<Type_> generics = null) {
        this.baseType_ = new BaseType_(name, bits);
        if (generics == null) {
            generics = new List<Type_>();
        }
        this.generics = generics;
    }

    public Type_(string name, List<Type_> generics = null) {
        this.baseType_ = new BaseType_(name);
        if (generics == null) {
            generics = new List<Type_>();
        }
        this.generics = generics;
    }

    public Type_ WithGenerics(List<Type_> generics) {
        return new Type_(baseType_, generics);
    }

    public BaseType_ GetBaseType_() {
        return baseType_;
    }

    public List<Type_> GetGenerics() {
        return generics;
    }

    public bool IsConvertibleTo(Type_ other) {
        if (baseType_.IsConvertibleTo(other.GetBaseType_())) {
            // maybe make casting of generics automatic later?
            return GenericsEqual(other);
        }
        return false;
    }

    public bool Equals(Type_ other) {
        if (baseType_.Equals(other.GetBaseType_())) {
            return GenericsEqual(other);
        }
        return false;
    }

    public bool GenericsEqual(Type_ other) {
        return Utils.ListEqual<Type_>(generics, other.GetGenerics());
    }

    public override string ToString() {
        string genericStr = "";
        bool first = true;
        foreach (Type_ generic in generics){
            if (!first) {
                genericStr += ", ";
            }
            genericStr += generic.ToString();
            first = false;
        }
        return Utils.WrapName(
            baseType_.ToString(), genericStr, "<", ">"
        );
    }
}

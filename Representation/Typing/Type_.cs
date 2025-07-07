using CsJSONTools;

namespace Epsilon;
public class Type_ : IEquatable<Type_> {
    public static Type_ String() {
        return new Type_("Byte").ArrayOf();
    }

    readonly BaseType_ baseType_;
    readonly List<Type_> generics;

    public static Type_ Any() {
        return new Type_("Any");
    }

    public static bool AreCompatible(Type_ a, Type_ b) {
        if (a.Matches(b)) return true;
        BaseType_ abt = a.GetBaseType_();
        BaseType_ bbt = b.GetBaseType_();
        if (abt.IsAny() || bbt.IsAny()) return true;
        if (a.HasGenerics() || b.HasGenerics())
            return false;
        bool aToB = abt.IsConvertibleTo(bbt);
        bool bToA = bbt.IsConvertibleTo(abt);
        return aToB || bToA;
    }

    public static Type_ CommonOrNull(Type_ a, Type_ b) {
        if (a.Matches(b)) return a;
        if (a.HasGenerics() || b.HasGenerics()) return null;
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
            return null;
        }
    }

    public static Type_ CommonNonNull(IToken token, Type_ a, Type_ b) {
        Type_ result = CommonOrNull(a, b);
        if (result == null) {
            throw new SyntaxErrorException(
                $"Cannot find common type between {a} and {b}", token
            );
        }
        return result;
    }

    public static Type_ CommonSpecificNullable(Type_ a, Type_ b, string name) {
        if (a.Matches(b)) {
            if (a.GetBaseType_().GetName()==name || b.GetBaseType_().GetName()==name)
                return a;
        }
        if (a.HasGenerics() || b.HasGenerics()) return null;
        int? abits = a.GetBaseType_().GetBits();
        int? bbits = b.GetBaseType_().GetBits();
        int? bits;
        if (abits == null) {
            bits = bbits;
        } else if (bbits == null) {
            bits = abits;
        } else {
            bits = Math.Max(abits.Value, bbits.Value);
        }
        return new Type_(name, bits);
    }

    public static Type_ CommonSpecificNonNull(IToken token, Type_ a, Type_ b, string name) {
        Type_ result = CommonSpecificNullable(a, b, name);
        if (result == null) {
            throw new SyntaxErrorException(
                $"Cannot find common type '{name}' between {a} and {b}", token
            );
        }
        return result;
    }

    public Type_(BaseType_ baseType_, List<Type_> generics = null) {
        this.baseType_ = baseType_;
        this.generics = generics ?? [];
        CheckGenerics(baseType_, this.generics);
    }

    public Type_(string name, int? bits, List<Type_> generics = null) {
        baseType_ = new BaseType_(name, bits);
        this.generics = generics ?? [];
        CheckGenerics(baseType_, this.generics);
    }

    public Type_(string name, List<Type_> generics = null) {
        baseType_ = new BaseType_(name);
        this.generics = generics ?? [];
        CheckGenerics(baseType_, this.generics);
    }

    void CheckGenerics(BaseType_ baseType_, List<Type_> generics) {
        if (generics.Count != baseType_.GenericsAmount()) {
            throw new IllegalType_Exception(
                $"Incorrect number of generics on base type {baseType_} (got {generics.Count}, expected {baseType_.GenericsAmount()})"
            );
        }
        if (StructsCtx.AreStructsLoaded()) {
            VerifyValidPoly(recursive: false);
        }
    }

    public void VerifyValidPoly(bool recursive) {
        if (baseType_.GetName() == "Poly") {
            Struct struct_ = StructsCtx.GetStructFromType_(generics[0]);
            if (struct_ == null) {
                throw new IllegalType_Exception(
                    $"Poly type generic must be a struct, {generics[0]} isn't"
                );
            }
            if (!struct_.IsSuper()) {
                throw new IllegalType_Exception(
                    $"Poly type generic must be annotated @super, {generics[0]} isn't"
                );
            }
        }
        if (recursive) {
            foreach (Type_ generic in generics) {
                generic.VerifyValidPoly(recursive: true);
            }
        }
    }

    public Type_ WithGenerics(List<Type_> generics) {
        return new Type_(baseType_, generics);
    }

    public Type_ OptionalOf() {
        return new Type_("Optional", [this]);
    }

    public Type_ ArrayOf() {
        return new Type_("Array", [this]);
    }

    public Type_ PolyOf() {
        return new Type_("Poly", [this]);
    }

    public Type_ UnwrapOptional() {
        if (baseType_.GetName() == "Optional") return generics[0];
        return this;
    }

    public Type_ UnwrapOptional(out bool was) {
        if (baseType_.GetName() == "Optional") {
            was = true;
            return generics[0];
        }
        was = false;
        return this;
    }

    public Type_ UnwrapPoly() {
        if (baseType_.GetName() == "Poly") return generics[0];
        return this;
    }

    public BaseType_ GetBaseType_() {
        return baseType_;
    }

    public List<Type_> GetGenerics() {
        return generics;
    }

    public Type_ GetGeneric(int i) {
        return generics[i];
    }

    public bool HasGenerics() {
        return generics.Count > 0;
    }

    public int GenericCount() {
        return generics.Count;
    }

    bool IsConvertibleNullTo(Type_ other) {
        if (baseType_.GetName() != "Null") return false;
        return other.GetBaseType_().GetName() == "Optional";
    }

    bool IsConvertibleToPolySub(Type_ other) {
        Struct source = StructsCtx.GetStructFromMaybePolyType_(this);
        if (source == null) return false;
        if (other.GetBaseType_().GetName() == "Struct") return true;
        Struct dest = StructsCtx.GetStructFromPolyType_(other);
        if (dest == null) return false;
        return source.ExtendList().Contains(dest);
    }

    static void UnwrapOptionalConversion(Type_ from, Type_ to, out Type_ fromOut, out Type_ toOut, out bool toWasOptional) {
        toWasOptional = false;
        while (true) {
            to = to.UnwrapOptional(out bool unwrappedLayer);
            if (!unwrappedLayer) break;
            toWasOptional = true;
            from = from.UnwrapOptional();
        }
        fromOut = from;
        toOut = to;
    }

    public bool IsConvertibleTo(Type_ other) {
        Type_ this_ = this;
        if (baseType_.IsAny() || other.GetBaseType_().IsAny())
            return true;
        if (this_.IsConvertibleNullTo(other)) return true;
        // conversion to optional is always implicit
        UnwrapOptionalConversion(this_, other, out this_, out other, out bool _);
        if (this_.IsConvertibleToPolySub(other)) return true;
        if (this_.HasGenerics()) return this_.Matches(other);
        if (other.HasGenerics()) return false;
        return this_.GetBaseType_().IsConvertibleTo(other.GetBaseType_());
    }

    public bool IsEquivalentTo(Type_ other) {
        BaseType_ otherBaseType_ = other.GetBaseType_();
        if (HasGenerics()) {
            if (!baseType_.Equals(otherBaseType_)) return false;
            List<Type_> otherGenerics = other.GetGenerics();
            if (generics.Count != otherGenerics.Count) return false;
            for (int i = 0; i < generics.Count; i++) {
                if (!generics[i].IsEquivalentTo(otherGenerics[i]))
                    return false;
            }
            return true;
        } else {
            return baseType_.IsEquivalentTo(otherBaseType_);
        }
    }

    bool IsCastablePolyToOptional(Type_ other, bool otherWasOptional) {
        if (!otherWasOptional) return false;
        Struct dest = StructsCtx.GetStructFromMaybePolyType_(other);
        if (dest == null) return false;
        if (GetBaseType_().GetName() == "Struct") return true;
        Struct source = StructsCtx.GetStructFromPolyType_(this);
        if (source == null) return false;
        return dest.ExtendList().Contains(source);
    }

    public bool IsCastableTo(Type_ other) {
        if (IsConvertibleTo(other)) return true;
        Type_ this_ = this;
        // conversion to optional is always implicit
        UnwrapOptionalConversion(this_, other, out this_, out other, out bool otherWasOptional);
        if (this_.IsCastablePolyToOptional(other, otherWasOptional)) return true;
        if (this_.HasGenerics()) return this_.Matches(other);
        if (other.HasGenerics()) return false;
        if (this_.GetBaseType_().IsCastableTo(other.GetBaseType_()))
            return true;
        return false;
    }

    public bool Equals(Type_ other) {
        return baseType_.Equals(other.GetBaseType_()) && GenericsEqual(other);
    }

    public bool Matches(Type_ other) {
        BaseType_ otherBaseType_ = other.GetBaseType_();
        if (baseType_.IsAny() || otherBaseType_.IsAny())
            return true;
        return baseType_.Equals(other.GetBaseType_()) && GenericsMatching(other);
    }

    public bool IsGreaterThan(Type_ other) {
        bool thisIsAny = baseType_.IsAny();
        bool otherIsAny = other.GetBaseType_().IsAny();
        if (thisIsAny && otherIsAny) return false;
        if (thisIsAny) return true;
        if (!other.IsConvertibleTo(this)) return false;
        if (!IsConvertibleTo(other)) return true;
        BaseType_ otherBaseType_ = other.GetBaseType_();
        if (baseType_.GetName() != otherBaseType_.GetName()) return false;
        int? thisBits = baseType_.GetBits();
        int? otherBits = otherBaseType_.GetBits();
        if (!thisBits.HasValue || !otherBits.HasValue) return false;
        return thisBits.Value > otherBits.Value;
    }

    public bool GenericsEqual(Type_ other) {
        return generics.SequenceEqual(other.GetGenerics());
    }

    public bool GenericsMatching(Type_ other) {
        return Enumerable.Zip(generics, other.GetGenerics(), (a, b) => a.Matches(b)).All(a => a);
    }

    public override string ToString() {
        if (generics.Count == 0) return baseType_.ToString();
        string genericStr = "";
        bool first = true;
        foreach (Type_ generic in generics) {
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

    public IJSONValue GetJSON() {
        return new JSONObject {
            ["name"] = new JSONString(baseType_.GetName()),
            ["bits"] = JSONInt.OrNull(baseType_.GetBitsOrDefaultIfMeaningful()),
            ["generics"] = new JSONList(generics.Select(generic => generic.GetJSON()))
        };
    }
}

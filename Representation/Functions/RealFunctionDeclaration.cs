using CsJSONTools;

namespace Epsilon;
public abstract class RealFunctionDeclaration : FunctionDeclaration, IEquatable<RealFunctionDeclaration> {
    protected abstract Type_ _GetReturnType_();
    public abstract string GetCallee();
    public abstract string GetSourcePath();
    public abstract bool TakesOwnership();
    public abstract bool ResultInParams();

    public Type_ GetReturnType_() {
        if (DoesReturnVoid()) {
            throw new InvalidOperationException("Void function has no return type_");
        } else {
            return _GetReturnType_();
        }
    }

    public JSONObject GetJSON() {
        return new JSONObject {
            ["id"] = new JSONString(GetID()),
            ["callee"] = new JSONString(GetCallee()),
            ["arguments"] = new JSONList(GetArguments().Select(
                argument => argument.GetJSON()
            )),
            ["return_type_"] = DoesReturnVoid() ? new JSONNull() : GetReturnType_().GetJSON(),
            ["takes_ownership"] = new JSONBool(TakesOwnership()),
            ["result_in_params"] = new JSONBool(ResultInParams())
        };
    }

    public bool Equals(RealFunctionDeclaration other) {
        bool arv = DoesReturnVoid();
        bool brv = other.DoesReturnVoid();
        if (arv ^ brv) {
            return false;
        } else if (!arv && !brv) {
            if (!GetReturnType_().Equals(other.GetReturnType_())) return false;
        }
        if (GetCallee() != other.GetCallee()) return false;
        if (TakesOwnership() != other.TakesOwnership()) return false;
        if (ResultInParams() != other.ResultInParams()) return false;
        if (!GetArguments().SequenceEqual(other.GetArguments())) return false;
        if (GetSource() != other.GetSource()) return false;
        return GetPattern().Equals(other.GetPattern());
    }

    protected sealed override Type_ _GetReturnType_(List<IValueToken> tokens) {
        return _GetReturnType_();
    }

    public bool IsMain() {
        return GetCallee() == "main";
    }

    public bool IsPrivate() {
        IPatternSegment firstSeg = GetPattern().GetSegments().GetOr(0);
        if (firstSeg is not UnitPatternSegment<string> unitSeg) return false;
        return unitSeg.GetValue().StartsWith('_');
    }
}

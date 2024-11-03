using CsJSONTools;

namespace Epsilon;
public class ConstantSubconfig : ISubconfig {
    readonly IEnumerable<string> parts;

    public ConstantSubconfig(IEnumerable<string> parts) {
        this.parts = parts;
    }

    public ConstantSubconfig(ShapedJSON obj) {
        obj = obj.ToShape(new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"parts", new JSONListShape(new JSONStringShape())}
        }));

        parts = obj["parts"].IterList().Select(part => part.GetString());
    }

    public IEnumerable<string> ToParts() {
        return parts;
    }

    public JSONObject GetJSON() {
        JSONObject obj = new() {
            ["type"] = new JSONString("constant"),
            ["parts"] = new JSONList(parts.Select(part => new JSONString(part)))
        };
        return obj;
    }
}

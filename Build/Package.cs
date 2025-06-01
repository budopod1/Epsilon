using System.Runtime.InteropServices.JavaScript;
using CsJSONTools;

namespace Epsilon;

public class Package(string name, string path, string source) {
    public static IJSONShape PackagesShape { get => _PackagesShape; }
    static readonly IJSONShape _PackagesShape;

    public readonly string Name = name;
    public readonly string Path = path;
    public readonly string Source = source;

    static Package() {
        _PackagesShape = new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"packages", new JSONListShape(new JSONObjectShape(new Dictionary<string, IJSONShape> {
                {"name", new JSONStringShape()},
                {"path", new JSONStringShape()},
                {"source", new JSONNullableShape(new JSONStringShape())}
            }))},
        });
    }

    public static Package FromJSON(ShapedJSON json) {
        return new Package(
            json["name"].GetString(),
            json["path"].GetString(),
            json["source"].GetStringOrNull()
        );
    }

    public JSONObject GetJSON() {
        return new JSONObject {
            {"name", new JSONString(Name)},
            {"path", new JSONString(Path)},
            {"source", JSONString.OrNull(Source)}
        };
    }
}

using CsJSONTools;

namespace Epsilon;

public class Package(string name, string path, string source, IEnumerable<string> dependencies) {
    public static IJSONShape PackagesShape { get => _PackagesShape; }
    static readonly IJSONShape _PackagesShape;

    public readonly string Name = name;
    public readonly string Path = path;
    public readonly string Source = source;
    public readonly IEnumerable<string> Dependencies = dependencies;

    static Package() {
        _PackagesShape = new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"packages", new JSONListShape(new JSONObjectShape(new Dictionary<string, IJSONShape> {
                {"name", new JSONStringShape()},
                {"path", new JSONStringShape()},
                {"source", new JSONNullableShape(new JSONStringShape())},
                {"dependencies", new JSONListShape(new JSONStringShape())}
            }))},
        });
    }

    public static Package FromJSON(ShapedJSON json) {
        return new Package(
            json["name"].GetString(),
            json["path"].GetString(),
            json["source"].GetStringOrNull(),
            json["dependencies"].IterList().Select(item => item.GetString())
        );
    }

    public JSONObject GetJSON() {
        return new JSONObject {
            {"name", new JSONString(Name)},
            {"path", new JSONString(Path)},
            {"source", JSONString.OrNull(Source)},
            {"dependencies", new JSONList(
                Dependencies.Select(dependency => new JSONString(dependency))
            )}
        };
    }
}

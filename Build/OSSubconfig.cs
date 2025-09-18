using System.Runtime.InteropServices;
using CsJSONTools;

namespace Epsilon;
public class OSSubconfig : ISubconfig {
    readonly IEnumerable<string> parts;
    readonly string os;

    public OSSubconfig(IEnumerable<string> parts, string os) {
        this.parts = parts;
        this.os = os;
    }

    public OSSubconfig(ShapedJSON obj) {
        obj = obj.ToShape(new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"parts", new JSONListShape(new JSONStringShape())},
            {"os", new JSONStringShape()}
        }));

        parts = obj["parts"].IterList().Select(part => part.GetString());
        os = obj["os"].GetString();
    }

    static string GetOSName() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            return "linux";
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            return "macos";
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) {
            return "freebsd";
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return "windows";
        } else {
            return "unknown";
        }
    }

    public IEnumerable<string> ToParts() {
        if (GetOSName() == os) {
            return parts;
        } else {
            return [];
        }
    }

    public JSONObject GetJSON() {
        JSONObject obj = new() {
            ["type"] = new JSONString("os"),
            ["parts"] = new JSONList(parts.Select(part => new JSONString(part))),
            ["os"] = new JSONString(os)
        };
        return obj;
    }
}

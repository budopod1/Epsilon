using CsJSONTools;

namespace Epsilon;
public interface ISubconfig {
    IEnumerable<string> ToParts();
    JSONObject GetJSON();
}

using CsJSONTools;

public interface ISubconfig {
    IEnumerable<string> ToParts();
    JSONObject GetJSON();
}

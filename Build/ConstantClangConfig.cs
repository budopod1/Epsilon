using System;

public class ConstantClangConfig : IClangConfig {
    string config;

    public ConstantClangConfig(string config) {
        this.config = config;
    }

    public string Stringify() {
        return config;
    }

    public JSONObject GetJSON() {
        JSONObject obj = new JSONObject();
        obj["type"] = new JSONString("constant");
        obj["config"] = new JSONString(config);
        return obj;
    }
}

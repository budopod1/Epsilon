using System;
using System.Linq;
using System.Collections.Generic;

public class ConstantClangConfig : IClangConfig {
    IEnumerable<string> parts;

    public ConstantClangConfig(IEnumerable<string> parts) {
        this.parts = parts;
    }

    public ConstantClangConfig(ShapedJSON obj) {
        obj = obj.ToShape(new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"parts", new JSONListShape(new JSONStringShape())}
        }));
        
        parts = obj["parts"].IterList().Select(part => part.GetString());
    }

    public IEnumerable<string> ToParts() {
        return parts;
    }

    public JSONObject GetJSON() {
        JSONObject obj = new JSONObject();
        obj["type"] = new JSONString("constant");
        obj["parts"] = new JSONList(parts.Select(part => new JSONString(part)));
        return obj;
    }
}

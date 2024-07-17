using System;
using System.Collections.Generic;

public interface IClangConfig {
    IEnumerable<string> ToParts();
    JSONObject GetJSON();
}

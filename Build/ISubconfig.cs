using System;
using System.Collections.Generic;

public interface ISubconfig {
    IEnumerable<string> ToParts();
    JSONObject GetJSON();
}

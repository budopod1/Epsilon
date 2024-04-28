using System;
using System.Linq;
using System.Collections.Generic;

public static class Extensions {
    public static Dictionary<TKey, (TValue1, TValue2)> MergeToPairs<TKey, TValue1, TValue2>(this Dictionary<TKey, TValue1> d1, Dictionary<TKey, TValue2> d2, Func<TValue1> default1, Func<TValue2> default2) {
        return d1.Keys.Concat(d1.Keys).Distinct().ToDictionary(key => key, key => (
            d1.GetOr(key, default1), d2.GetOr(key, default2)
        ));
    }

    public static TValue GetOr<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> default_) {
        return dict.ContainsKey(key) ? dict[key] : default_();
    }
}

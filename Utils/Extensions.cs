using System;
using System.Linq;
using System.Collections.Generic;

public static class Extensions {
    public static List<T> Slice<T>(this IList<T> list, int length) {
        List<T> result = new List<T>();
        for (int i = 0; i < length; i++) {
            result.Add(list[i]);
        }
        return result;
    }
    public static List<T> Slice<T>(this IList<T> list, int start, int length) {
        List<T> result = new List<T>();
        for (int i = start; i < length; i++) {
            result.Add(list[i]);
        }
        return result;
    }

    public static T GetOr<T>(this IList<T> list, int idx, T default_=default(T)) {
        if (idx >= list.Count) return default_;
        return list[idx];
    }

    public static TValue GetOr<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue default_=default(TValue)) {
        if (dict.TryGetValue(key, out TValue val)) {
            return val;
        } else {
            return default_;
        }
    }

    public static Dictionary<TKey, (TValue1, TValue2)> MergeToPairs<TKey, TValue1, TValue2>(this Dictionary<TKey, TValue1> d1, Dictionary<TKey, TValue2> d2, Func<TValue1> default1, Func<TValue2> default2) {
        return d1.Keys.Concat(d1.Keys).Distinct().ToDictionary(key => key, key => (
            d1.GetOr(key, default1), d2.GetOr(key, default2)
        ));
    }

    public static Dictionary<TKey, IEnumerable<TValue>> ToDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> source) {
        return source.ToDictionary(grouping => grouping.Key, grouping => (IEnumerable<TValue>)grouping);
    }

    public static TValue GetOr<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> default_) {
        return dict.ContainsKey(key) ? dict[key] : default_();
    }

    public static IEnumerable<TItem> GetOrEmpty<TKey, TItem>(this Dictionary<TKey, IEnumerable<TItem>> dict, TKey key) {
        return dict.ContainsKey(key) ? dict[key] : new TItem[0];
    }

    public static Dictionary<TKey, TSource> ToDictionary2<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
        var result = new Dictionary<TKey, TSource>();
        foreach (TSource item in source) {
            TKey key = keySelector(item);
            if (result.ContainsKey(key)) {
                throw new DuplicateKeyException<TKey, TSource>(
                    key, result[key], item
                );
            }
            result[key] = item;
        }
        return result;
    }

    public static Dictionary<TKey, TValue> ToDictionary2<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) {
        var result = new Dictionary<TKey, TValue>();
        var sourceItems = new Dictionary<TKey, TSource>();
        foreach (TSource item in source) {
            TKey key = keySelector(item);
            if (sourceItems.ContainsKey(key)) {
                throw new DuplicateKeyException<TKey, TSource>(
                    key, sourceItems[key], item
                );
            }
            result[key] = valueSelector(item);
            sourceItems[key] = item;
        }
        return result;
    }

    public static (IEnumerable<T>, IEnumerable<TSub>) ParitionSubclass<T, TSub>(this IEnumerable<T> vals) where TSub : T {
        SubclassPartitioningManager<T, TSub> manager = new SubclassPartitioningManager<T, TSub>(vals);
        return (PartitionSubclassOther(manager), PartitionSubclassSub(manager));
    }

    static IEnumerable<T> PartitionSubclassOther<T, TSub>(SubclassPartitioningManager<T, TSub> manager) where TSub : T {
        while (true) {
            T next;
            try {
                next = manager.RequestOther();
            } catch (InvalidOperationException) {
                break;
            }
            yield return next;
        }
    }

    static IEnumerable<TSub> PartitionSubclassSub<T, TSub>(SubclassPartitioningManager<T, TSub> manager) where TSub : T {
        while (true) {
            TSub next;
            try {
                next = manager.RequestSub();
            } catch (InvalidOperationException) {
                break;
            }
            yield return next;
        }
    }

    class SubclassPartitioningManager<T, TSub> where TSub : T {
        IEnumerator<T> vals;
        Queue<TSub> subBuffer = new Queue<TSub>();
        Queue<T> otherBuffer = new Queue<T>();

        public SubclassPartitioningManager(IEnumerable<T> vals) {
            this.vals = vals.GetEnumerator();
        }

        public TSub RequestSub() {
            if (subBuffer.Count == 0) {
                while (vals.MoveNext()) {
                    T val = vals.Current;
                    if (val is TSub) {
                        return (TSub)val;
                    } else {
                        otherBuffer.Enqueue(val);
                    }
                }
                throw new InvalidOperationException("No more subs remain");
            } else {
                return subBuffer.Dequeue();
            }
        }

        public T RequestOther() {
            if (otherBuffer.Count == 0) {
                while (vals.MoveNext()) {
                    T val = vals.Current;
                    if (val is TSub) {
                        subBuffer.Enqueue((TSub)val);
                    } else {
                        return val;
                    }
                }
                throw new InvalidOperationException("No more others remain");
            } else {
                return otherBuffer.Dequeue();
            }
        }
    }

    /*
    Just a utility method for debugging
    public static void DetectNontransitiveSorting<T>(this List<T> items, IComparer<T> comparer) {
        for (int i = 0; i < items.Count; i++) {
            for (int j = i + 1; j < items.Count; j++) {
                T a = items[i];
                T b = items[j];
                int comparison = comparer.Compare(a, b);
                if (comparison > 0) {
                    Log.Tmp("fail", a, b, comparison);
                }
            }
        }
        Log.Tmp("end");
    }
    */
}

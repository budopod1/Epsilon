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

    public static Dictionary<TKey, TSource> ToDictionary2<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
        var result = new Dictionary<TKey, TSource>();
        foreach (TSource val in source) {
            TKey key = keySelector(val);
            if (result.ContainsKey(key)) {
                throw new DuplicateKeyException<TKey, TSource>(key, result[key], val);
            }
            result[key] = val;
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
}

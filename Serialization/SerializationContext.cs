using CsJSONTools;
using System.Collections;
using System.Runtime.Serialization;

public class SerializationContext {
    private readonly SerializationContext parent = null;
    private readonly List<int> varsHere = [];
    private readonly JSONList json = [];
    private readonly Dictionary<ISerializableToken, int> cache = [];

    SerializationContext(SerializationContext parent) {
        this.parent = parent;
    }

    public SerializationContext GetParent() {
        return parent;
    }

    public void RegisterVarDecl(int var) {
        varsHere.Add(var);
    }

    public int Serialize(ISerializableToken token) {
        if (cache.TryGetValue(token, out var value))
            return value;
        return cache[token] = token.UncachedSerialize(this);
    }

    public static IJSONValue SerializeValue(SerializationContext parentCtx, IValueToken val) {
        SerializationContext ctx = new(parentCtx);
        int result = ctx.Serialize(val);
        return new JSONObject {
            ["ir"] = ctx.GetJSON(),
            ["val"] = new JSONInt(result)
        };
    }

    public static IJSONValue SerializeBlock(SerializationContext parentCtx, CodeBlock block) {
        SerializationContext ctx = new(parentCtx);
        foreach (IToken child in block) {
            ctx.Serialize((ISerializableToken)child);
        }
        return new JSONObject {
            ["ir"] = ctx.GetJSON(),
            ["vars_here"] = new JSONList(ctx.GetVarsHere().Select(id => new JSONInt(id))),
            ["intialized_vars"] = new JSONList(ctx.GetInitializedVars().Select(id => new JSONInt(id))),
            ["does_terminate_function"] = new JSONBool(block.DoesTerminateFunction()),
        };
    }

    public IEnumerable<int> GetVarsHere() {
        return varsHere;
    }

    public IEnumerable<int> GetInitializedVars() {
        IEnumerable<int> result = GetVarsHere();
        if (parent == null) {
            return result;
        } else {
            return result.Concat(parent.GetInitializedVars());
        }
    }

    public IJSONValue ObjToJSON(object obj) {
        if (obj == null) {
            return new JSONNull();
        } else if (obj is IJSONValue json) {
            return json;
        } else if (obj is int i) {
            return new JSONInt(i);
        } else if (obj is double d) {
            return new JSONDouble(d);
        } else if (obj is string str) {
            return new JSONString(str);
        } else if (obj is Type_ type_) {
            return type_.GetJSON();
        } else if (obj is IConstant constant) {
            return constant.GetJSON();
        } else if (obj is CodeBlock block) {
            return SerializeBlock(this, block);
        } else if (obj is IValueToken val) {
            return SerializeValue(this, val);
        } else if (obj is IDictionary dict) {
            JSONObject result = [];
            foreach (DictionaryEntry entry in dict) {
                result[(string)entry.Key] = ObjToJSON(entry.Value);
            }
            return result;
        } else if (obj is IEnumerable enumerable) {
            JSONList result = [];
            foreach (object sub in enumerable) {
                result.Add(ObjToJSON(sub));
            }
            return result;
        } else {
            throw new InvalidOperationException($"Don't know how to add value {obj} to SerializableInstruction");
        }
    }

    JSONList GetJSON() {
        return json;
    }

    public int AddInstructionJSON(JSONObject obj) {
        json.Add(obj);
        return json.Count - 1;
    }
}

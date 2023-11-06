using System;
using System.Collections.Generic;
using System.Linq;

public class SerializableInstruction {
    JSONObject obj = new JSONObject();
    
    public SerializableInstruction(string name, List<int> parameters=null, Type_ type_=null) {
        obj["name"] = new JSONString(name);
        if (parameters == null) {
            obj["parameters"] = new JSONList();
        } else {
            obj["parameters"] = new JSONList(parameters.Select(
                param=>new JSONInt(param)
            ));
        }
        if (type_ != null) obj["type_"] = type_.GetJSON();
    }

    public SerializableInstruction(IParentToken token,
                                   SerializationContext context) {
        obj["name"] = new JSONString(Utils.CammelToSnake(
            token.GetType().Name
        ));
        JSONList parameters = new JSONList();
        for (int i = 0; i < token.Count; i++) {
            ISerializableToken sub = token[i] as ISerializableToken;
            if (sub != null) parameters.Add(new JSONInt(sub.Serialize(context)));
        }
        obj["parameters"] = parameters;
        IValueToken valueToken = token as IValueToken;
        if (valueToken != null) obj["type_"] = valueToken.GetType_().GetJSON();
    }

    public SerializableInstruction(IToken token) {
        obj["name"] = new JSONString(Utils.CammelToSnake(
            token.GetType().Name
        ));
        obj["parameters"] = new JSONList();
        IValueToken valueToken = token as IValueToken;
        if (valueToken != null) obj["type_"] = valueToken.GetType_().GetJSON();
    }

    public SerializableInstruction AddData(string name, IJSONValue value) {
        obj[name] = value;
        return this;
    }

    public IJSONValue GetJSON() {
        return obj;
    }
}

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class EPSLPROJ {
    public long CompileStartTime;
    public List<string> EPSLSPECS;
    public static IJSONShape Shape { get => _Shape; }
    static IJSONShape _Shape;

    static EPSLPROJ() {
        _Shape = new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"compile_start_time_1", new JSONIntShape()},
            {"compile_start_time_2", new JSONIntShape()},
            {"epslspecs", new JSONListShape(new JSONStringShape())}
        });
    }
    
    public EPSLPROJ(long compileStartTime, List<string> epslspecs) {
        CompileStartTime = compileStartTime;
        EPSLSPECS = epslspecs;
    }

    public static EPSLPROJ FromText(IJSONValue jsonValue) {
        ShapedJSON json = new ShapedJSON(jsonValue, Shape);
        int a = json["compile_start_time_1"].GetInt();
        int b = json["compile_start_time_2"].GetInt();
        long compileStartTime = Utils.IntsToLong((a, b));
        List<string> epslspecs = new List<string>();
        foreach (ShapedJSON epslspec in json["epslspecs"].IterList()) {
            epslspecs.Add(epslspec.GetString());
        }
        return new EPSLPROJ(compileStartTime, epslspecs);
    }

    public void ToFile(string path) {
        JSONObject obj = new JSONObject();
        (int a, int b) = Utils.LongToInts(CompileStartTime);
        obj["compile_start_time_1"] = new JSONInt(a);
        obj["compile_start_time_2"] = new JSONInt(b);
        obj["epslspecs"] = new JSONList(EPSLSPECS.Select(
            epslspec => new JSONString(epslspec)
        ));
        string fileText = obj.ToJSON();
        using (StreamWriter file = new StreamWriter(path)) {
            file.Write(fileText);
        }
    }
}

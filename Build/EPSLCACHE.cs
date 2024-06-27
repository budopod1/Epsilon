using System;
using System.Linq;
using System.Collections.Generic;

public class EPSLCACHE {
    public bool IsFromFile = false;
    public string Path;
    public long? CompileStartTime = null;
    public List<string> EPSLSPECS = new List<string>();

    public static IJSONShape Shape { get => _Shape; }
    static IJSONShape _Shape;

    static EPSLCACHE() {
        _Shape = new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"compile_start_time_1", new JSONNullableShape(new JSONIntShape())},
            {"compile_start_time_2", new JSONNullableShape(new JSONIntShape())},
            {"epslspecs", new JSONListShape(new JSONStringShape())}
        });
    }

    public EPSLCACHE(string path, long? compileStartTime, List<string> epslspecs, bool isFromFile) {
        Path = path;
        CompileStartTime = compileStartTime;
        EPSLSPECS = epslspecs;
        IsFromFile = isFromFile;
    }

    public EPSLCACHE(string path) {
        Path = path;
    }

    public static EPSLCACHE FromText(string path, IJSONValue jsonValue) {
        ShapedJSON json = new ShapedJSON(jsonValue, Shape);
        int? a = json["compile_start_time_1"].GetIntOrNull();
        int? b = json["compile_start_time_2"].GetIntOrNull();
        long? compileStartTime = Utils.IntsToLong((a, b));
        List<string> epslspecs = json["epslspecs"].IterList().Select(
            epslspec => epslspec.GetString()
        ).ToList();
        return new EPSLCACHE(path, compileStartTime, epslspecs, isFromFile: true);
    }

    public static CacheMode ParseCacheMode(string txt) {
        switch (txt) {
            case "dont-use":
                return CacheMode.DONTUSE;
            case "dont-load":
                return CacheMode.DONTLOAD;
            case "auto":
                return CacheMode.AUTO;
            case "always":
                return CacheMode.ALWAYS;
            default:
                throw new InvalidOperationException();
        }
    }

    public void ToFile() {
        JSONObject obj = new JSONObject();
        (int? a, int? b) = Utils.LongToInts(CompileStartTime);
        obj["compile_start_time_1"] = JSONInt.OrNull(a);
        obj["compile_start_time_2"] = JSONInt.OrNull(b);
        obj["epslspecs"] = new JSONList(EPSLSPECS.Select(
            epslspec => new JSONString(epslspec)));
        BJSONEnv.WriteFile(Path, obj);
    }
}
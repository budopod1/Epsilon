using System;
using System.Linq;
using System.Collections.Generic;

public class EPSLCACHE {
    public bool IsFromFile = false;
    public string Path;
    public long? CompileStartTime = null;
    public List<string> EPSLSPECS = [];
    public OutputType LastOutputType;

    public static IJSONShape Shape { get => _Shape; }
    static readonly IJSONShape _Shape;

    static EPSLCACHE() {
        _Shape = new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"compile_start_time_1", new JSONNullableShape(new JSONIntShape())},
            {"compile_start_time_2", new JSONNullableShape(new JSONIntShape())},
            {"epslspecs", new JSONListShape(new JSONStringShape())},
            {"output_type", new JSONWholeShape()}
        });
    }

    public EPSLCACHE(string path, long? compileStartTime, List<string> epslspecs, OutputType outputType, bool isFromFile) {
        Path = path;
        CompileStartTime = compileStartTime;
        EPSLSPECS = epslspecs;
        IsFromFile = isFromFile;
        LastOutputType = outputType;
    }

    public EPSLCACHE(string path) {
        Path = path;
        LastOutputType = OutputType.NONE;
    }

    public static EPSLCACHE FromText(string path, IJSONValue jsonValue) {
        ShapedJSON json = new(jsonValue, Shape);
        int? a = json["compile_start_time_1"].GetIntOrNull();
        int? b = json["compile_start_time_2"].GetIntOrNull();
        long? compileStartTime = Utils.IntsToLong((a, b));
        List<string> epslspecs = json["epslspecs"].IterList().Select(
            epslspec => epslspec.GetString()
        ).ToList();
        OutputType outputType = (OutputType)json["output_type"].GetInt();
        return new EPSLCACHE(path, compileStartTime, epslspecs, outputType, isFromFile: true);
    }

    public static bool MustDiscardCache(OutputType old, OutputType new_) {
        if (old == OutputType.NONE) return false;
        if (old == new_) return false;
        if (old.DoesRequireLLVM() != new_.DoesRequireLLVM()) return true;
        return false;
    }

    public void ToFile() {
        JSONObject obj = [];
        (int? a, int? b) = Utils.LongToInts(CompileStartTime);
        obj["compile_start_time_1"] = JSONInt.OrNull(a);
        obj["compile_start_time_2"] = JSONInt.OrNull(b);
        obj["epslspecs"] = new JSONList(EPSLSPECS.Select(
            epslspec => new JSONString(epslspec)));
        obj["output_type"] = new JSONInt((int)LastOutputType);
        BinJSONEnv.WriteFile(Path, obj);
    }
}

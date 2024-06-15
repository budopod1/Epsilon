using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class EPSLPROJ {
    public bool IsFromFile = false;
    public string Path;
    public long? CompileStartTime = null;
    public List<string> EPSLSPECS = new List<string>();
    public List<string> CommandOptions = new List<string>();
    public static IJSONShape Shape { get => _Shape; }
    static IJSONShape _Shape;

    static EPSLPROJ() {
        _Shape = new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"compile_start_time_1", new JSONNullableShape(new JSONIntShape())},
            {"compile_start_time_2", new JSONNullableShape(new JSONIntShape())},
            {"epslspecs", new JSONListShape(new JSONStringShape())},
            {"command_options", new JSONListShape(new JSONStringShape())}
        });
    }
    
    public EPSLPROJ(string path, long? compileStartTime, List<string> epslspecs, List<string> commandOptions, bool isFromFile) {
        Path = path;
        CompileStartTime = compileStartTime;
        EPSLSPECS = epslspecs;
        CommandOptions = commandOptions;
        IsFromFile = isFromFile;
    }

    public EPSLPROJ(string path, long? compileStartTime, List<string> epslspecs) {
        Path = path;
        CompileStartTime = compileStartTime;
        EPSLSPECS = epslspecs;
    }
    
    public EPSLPROJ(string path) {
        Path = path;
    }

    public static EPSLPROJ FromText(string path, IJSONValue jsonValue) {
        ShapedJSON json = new ShapedJSON(jsonValue, Shape);
        int? a = json["compile_start_time_1"].GetIntOrNull();
        int? b = json["compile_start_time_2"].GetIntOrNull();
        long? compileStartTime = Utils.IntsToLong((a, b));
        List<string> epslspecs = new List<string>();
        foreach (ShapedJSON epslspec in json["epslspecs"].IterList()) {
            epslspecs.Add(epslspec.GetString());
        }
        List<string> commandOptions = new List<string>();
        foreach (ShapedJSON commandOption in json["command_options"].IterList()) {
            commandOptions.Add(commandOption.GetString());
        }
        return new EPSLPROJ(path, compileStartTime, epslspecs, commandOptions, true);
    }

    public void ToFile() {
        JSONObject obj = new JSONObject();
        (int? a, int? b) = Utils.LongToInts(CompileStartTime);
        obj["compile_start_time_1"] = JSONInt.OrNull(a);
        obj["compile_start_time_2"] = JSONInt.OrNull(b);
        obj["epslspecs"] = new JSONList(EPSLSPECS.Select(
            epslspec => new JSONString(epslspec)
        ));
        obj["command_options"] = new JSONList(CommandOptions.Select(
            commandOption => new JSONString(commandOption)
        ));
        string fileText = obj.Stringify();
        using (StreamWriter file = new StreamWriter(Path)) {
            file.Write(fileText);
        }
    }
}

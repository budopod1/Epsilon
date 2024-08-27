public class EPSLPROJ {
    public string Path;
    public List<string> CommandOptions = [];
    public List<string> Libraries = [];
    public static IJSONShape Shape { get => _Shape; }
    static readonly IJSONShape _Shape;

    static EPSLPROJ() {
        _Shape = new JSONObjectShape(new Dictionary<string, IJSONShape> {
            {"command_options", new JSONListShape(new JSONStringShape())},
            {"libraries", new JSONListShape(new JSONStringShape())}
        });
    }

    public EPSLPROJ(string path, List<string> commandOptions, List<string> libraries) {
        Path = path;
        CommandOptions = commandOptions;
        Libraries = libraries;
    }

    public EPSLPROJ(string path, long? compileStartTime, List<string> epslspecs) {
        Path = path;
    }

    public EPSLPROJ(string path) {
        Path = path;
    }

    public static EPSLPROJ FromText(string path, IJSONValue jsonValue) {
        ShapedJSON json = new(jsonValue, Shape);
        List<string> commandOptions = json["command_options"].IterList()
            .Select(option => option.GetString()).ToList();
        List<string> libraries = json["libraries"].IterList()
            .Select(library => library.GetString()).ToList();
        return new EPSLPROJ(path, commandOptions, libraries);
    }

    public void ToFile() {
        JSONObject obj = new() {
            ["command_options"] = new JSONList(CommandOptions.Select(commandOption => new JSONString(commandOption))),
            ["libraries"] = new JSONList(Libraries.Select(library => new JSONString(library)))
        };
        PrettyPrintConfig printConfig = new(4, 60);
        string fileText = obj.PrettyPrint(printConfig);
        using StreamWriter file = new(Path);
        file.Write(fileText);
    }
}

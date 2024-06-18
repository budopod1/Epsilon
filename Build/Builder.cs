using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

public class Builder {
    public bool ALWAYS_PROJECT = false;
    public bool NEVER_PROJECT = false;
    public bool DISABLE_CACHE = false;
    public bool LINK_BUILTINS = true;
    public bool LINK_LIBRARIES = true;
    public IEnumerable<string> EXTRA_CLANG_OPTIONS = Enumerable.Empty<string>();
    public string EPSLLIBS;
    
    bool isProj = false;
    string currentFile = "";
    string currentText = "";
    long? lastBuildStartTime;
    Dictionary<string, string> libraries;
    Dictionary<string, FileTree> files;
    List<string> extensions = new List<string> {"epslspec", "epsl"};
    List<string> prefixes = new List<string> {"", "."};
    List<string> IRInUserDir;
    List<FileTree> unlinkedFiles;
    HashSet<Struct> structs;

    public Builder() {
        EPSLLIBS = Utils.JoinPaths(Utils.ProjectAbsolutePath(), "libs");
    }

    public ResultStatus LoadEPSLPROJ(string input, out EPSLPROJ projOut, bool allowNew=true) {
        EPSLPROJ proj = null;

        string projLocation = null;
        ResultStatus status1 = RunWrapped(() => {
            string projDirectory = Utils.GetFullPath(Utils.GetDirectoryName(input));
            string projName = Utils.GetFileNameWithoutExtension(input) + ".epslproj";
            projLocation = Utils.JoinPaths(projDirectory, projName);
        });
        if (status1 != ResultStatus.GOOD) {
            projOut = null;
            return status1;
        }

        if (NEVER_PROJECT) {
            projOut = new EPSLPROJ(projLocation);
            return status1;
        }
        
        ResultStatus status2 = RunWrapped(() => {
            if (Utils.FileExists(projLocation)) {
                currentFile = projLocation;
                proj = EPSLPROJ.FromText(projLocation, ParseJSONFile(projLocation));
                Log.Info("Loaded EPSLPROJ");
                isProj = true;
            }
        });

        // This C# version does not support the ??= operator
        projOut = proj ?? new EPSLPROJ(projLocation);

        if (!allowNew && proj == null) {
            projOut = null;
            Console.WriteLine("Cannot find requested EPSLPROJ");
            return ResultStatus.USERERR;
        }
        
        return status2;
    }

    public ResultStatus GetOutputLocation(string input, string extension, out string outputOut, out string outputNameOut) {
        string output = null;
        string outputName = null;
        ResultStatus status = RunWrapped(() => {
            string outputDir = Utils.GetFullPath(Utils.GetDirectoryName(input));
            outputName = Utils.SetExtension(Utils.GetFileName(input), null);
            if (outputName == "entry") outputName = "result";
            string outputFile = Utils.SetExtension(outputName, extension);
            output = Utils.JoinPaths(outputDir, outputFile);
        });
        outputOut = output;
        outputNameOut = outputName;
        return status;
    }

    public ResultStatus ReadyPackageFolder(string relFolder) {
        return RunWrapped(() => {
            string folder = Utils.GetFullPath(relFolder);
            if (Directory.Exists(folder)) {
                string[] subdirectories = Directory.GetDirectories(folder);
                if (subdirectories.Length > 0) {
                    throw new IOException("Cannot output package to folder with subdirectories");
                }
                string[] files = Directory.GetFiles(folder);
                foreach (string file in files) {
                    File.Delete(file);
                }
            } else if (File.Exists(folder)) {
                throw new IOException($"File already exists at specified package output location, {folder}");
            } else {
                Directory.CreateDirectory(folder);
            }
        });
    }

    public ResultStatus SaveEPSLSPEC(string path, EPSLSPEC epslspec) {
        return RunWrapped(() => {
            JSONObject json = epslspec.ToJSON(this);
            BJSONEnv.WriteFile(path, json);
        });
    }

    public ResultStatus RegisterLibraries(IEnumerable<string> relPaths) {
        return RunWrapped(() => {
            IEnumerable<string> paths = relPaths
                .Select(path => Utils.GetFullPath(path.Replace("%EPSLLIBS%", EPSLLIBS)))
                .Distinct();
            try {
                libraries = paths.ToDictionary2((path) => Utils.GetFileNameWithoutExtension(path));
            } catch (DuplicateKeyException<string, string> e) {
                throw new ProjectProblemException(
                    $"All libraries must have distinct names. Libaries '{e.Initial}' and '{e.Duplicate}' both have name '{e.Key}'."
                );
            }
        });
    }

    public ResultStatus Build(string inputRel, EPSLPROJ proj, out BuildInfo buildInfo) {
        Log.Info("Starting build of", inputRel);
        
        BuildInfo _buildInfo = null;
        
        ResultStatus status = RunWrapped(() => {
            long buildStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string input = Utils.GetFullPath(inputRel);
            string projectDirectory = Utils.GetDirectoryName(input);

            lastBuildStartTime = proj.CompileStartTime;
            List<string> lastGeneratedSPECs = proj.EPSLSPECS;
            isProj = !NEVER_PROJECT && proj.IsFromFile;

            currentFile = input;
            files = new Dictionary<string, FileTree>();
            FileTree tree = LoadFile(Utils.GetFileNameWithoutExtension(input), projectDirectory);

            Log.Status("Loading tree");
            LoadTree(tree, projectDirectory);
            DetermineIsProj();
            Log.Status("Transfering struct IDs");
            TransferStructIDs();
            Log.Status("Transfering declarations");
            TransferDeclarations();
            Log.Status("Transfering struct");
            TransferStructs();
            SetupOldCompilers();
            Log.Status("Confirming dependencies");
            ConfirmDependencies(projectDirectory);
            IRInUserDir = new List<string>();
            unlinkedFiles = new List<FileTree>();
            Log.Status("Writing IR");
            List<string> sections = ToIR();
            Log.Status("Saving EPSLSPECs");
            SaveEPSLSPECs();

            List<string> generatedSPECs = GetGeneratedEPSLSPECs().ToList();
            DeleteUnusedGeneratedSPECs(lastGeneratedSPECs, generatedSPECs);

            string fileWithMain = null;
            foreach (FileTree file in files.Values) {
                foreach (RealFunctionDeclaration declaration in file.Declarations) {
                    if (!declaration.IsMain()) continue;
                    if (fileWithMain == null) {
                        fileWithMain = file.Stemmed;
                    } else {
                        throw new ProjectProblemException($"No more than one main function is allowed; main functions found in both {fileWithMain} and {file.Stemmed}");
                    }
                }
            }

            List<string> arguments = new List<string> {
                "-o", Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-linked.bc"), "--"
            };
            if (LINK_BUILTINS) {
                arguments.Add(Utils.JoinPaths(EPSLLIBS, "builtins.bc"));
            }
            
            arguments.AddRange(sections);
            Log.Status("Linking LLVM");
            Utils.RunCommand("llvm-link", arguments);

            if (!IsProjMode()) {
                foreach (string path in IRInUserDir) {
                    Utils.TryDelete(path);
                }
            }

            if (IsProjMode()) {
                proj.CompileStartTime = buildStartTime;
                proj.EPSLSPECS = generatedSPECs;
                proj.ToFile();
                Log.Info("Saved updated EPSLPROJ");
            }

            IEnumerable<IClangConfig> extraClangConfig = EXTRA_CLANG_OPTIONS
                .Select(option => new ConstantClangConfig(option));
            IEnumerable<IClangConfig> clangConfig = files.Values
                .SelectMany(file => file.Compiler.GetClangConfig())
                .Concat(extraClangConfig);
            _buildInfo = new BuildInfo(
                tree, clangConfig, unlinkedFiles, fileWithMain
            );
        });

        buildInfo = _buildInfo;

        return status;
    }

    bool IsProjMode() {
        return !NEVER_PROJECT && (ALWAYS_PROJECT || isProj);
    }
    
    public ResultStatus RunWrapped(Action action) {
        try {
            action();
        } catch (SyntaxErrorException e) {
            ShowCompilationError(e, currentText, currentFile);
            return ResultStatus.USERERR;
        } catch (TargetInvocationException e) {
            Exception inner = e.InnerException;
            if (inner is SyntaxErrorException) {
                ShowCompilationError(
                    (SyntaxErrorException)inner, currentText, currentFile
                );
                return ResultStatus.USERERR;
            } else {
                ExceptionDispatchInfo.Capture(inner).Throw();
                // The next line won't be called, we just need it to keep the
                // C# compiler happy
                return ResultStatus.FAIL;
            }
        } catch (CommandFailureException e) {
            Console.WriteLine(e.Message);
            return ResultStatus.FAIL;
        } catch (FileNotFoundException e) {
            Console.Write(e.Message);
            if (e.FileName != null) {
                Console.Write(": " + e.FileName);
            }
            Console.WriteLine();
            return ResultStatus.USERERR;
        } catch (IOException e) {
            Console.Write("IO Error: ");
            Console.WriteLine(e.Message);
            return ResultStatus.USERERR;
        } catch (InvalidJSONException e) {
            JSONTools.ShowError(currentText, e, currentFile);
            return ResultStatus.USERERR;
        } catch (InvalidBJSONException e) {
            Console.WriteLine($"Error while reading BJSON file: {e.Message}");
            return ResultStatus.FAIL;
        } catch (ModuleNotFoundException e) {
            Console.WriteLine($"Cannot find requested module '{e.Path}'");
            return ResultStatus.USERERR;
        } catch (ProjectProblemException e) {
            Console.WriteLine(e.Problem);
            return ResultStatus.USERERR;
        }
        
        return ResultStatus.GOOD;
    }

    void SwitchFile(FileTree file) {
        currentFile = file.Path;
        currentText = file.Text;
    }

    void SwitchToOldFile(FileTree file) {
        currentFile = file.OldPath;
        currentText = file.OldText;
    }

    void CleanupSPEC(string path, ShapedJSON obj) {
        string ir = obj["ir"].GetString();
        string source = obj["source"].GetString();
        if (ir != null) {
            string irPath = Utils.JoinPaths(Utils.GetDirectoryName(path), ir);
            if (source != irPath) Utils.TryDelete(irPath);
        }
        Utils.TryDelete(path);
    }

    void CleanupSPEC(string path) {
        currentFile = path;
        IJSONValue jsonValue = ParseJSONFile(path);
        ShapedJSON obj = new ShapedJSON(jsonValue, SPECFileCompiler.Shape);
        CleanupSPEC(path, obj);
    }

    int LOAD_RETRY = 3;

    public DispatchedFile DispatchEPSLSPEC(string path, string _1, SPECFileCompiler _2) {
        currentFile = path;
        string stemmed = Utils.Stem(path);
        IJSONValue jsonValue = ParseJSONFile(path, out string fileText);
        ShapedJSON obj = new ShapedJSON(jsonValue, SPECFileCompiler.Shape);
        string source = Utils.GetFullPath(obj["source"].GetStringOrNull());
        string generatedEPSLSPEC = null;
        if (source != null) {
            if (NEVER_PROJECT || DISABLE_CACHE) return null;
            if (!Utils.FileExists(source)) {
                throw new InvalidSPECResourceException(
                    obj, path, source
                );
            }
            generatedEPSLSPEC = path;
            currentFile = source;
            if (lastBuildStartTime == null) {
                return DispatchPath(source, path, new SPECFileCompiler(
                    stemmed, fileText, obj
                ));
            }
            long fileModified = new DateTimeOffset(File.GetLastWriteTime(source)).ToUnixTimeSeconds();
            if (fileModified > lastBuildStartTime.Value) {
                return DispatchPath(source, path, new SPECFileCompiler(
                    stemmed, fileText, obj
                ));
            }
        }
        currentFile = path;
        string ir = obj["ir"].GetString();
        if (ir != null) {
            string irPath = Utils.JoinPaths(Utils.GetDirectoryName(path), ir);
            if (!Utils.FileExists(irPath)) {
                throw new InvalidSPECResourceException(
                    obj, path, irPath
                );
            }
        }
        return new DispatchedFile(
            new SPECFileCompiler(stemmed, fileText, obj), path, null, null,
            generatedEPSLSPEC
        );
    }

    public DispatchedFile DispatchEPSL(string path, string oldCompilerPath, SPECFileCompiler oldCompiler) {
        string fileText;
        currentFile = path;
        using (StreamReader file = new StreamReader(path)) {
            fileText = file.ReadToEnd();
        }
        currentText = fileText;
        string stemmed = Utils.Stem(path);
        IFileCompiler compiler = new EPSLFileCompiler(stemmed, fileText);
        return new DispatchedFile(compiler, path, oldCompilerPath, oldCompiler);
    }

    IEnumerable<string> FileLocations(string partialPath, string projDirectory) {
        foreach (string extension in extensions) {
            foreach (string prefix in prefixes) {
                string filename = prefix + partialPath + "." + extension;
                currentFile = filename;
                List<string> folders = new List<string> {projDirectory, EPSLLIBS};
                if (libraries.ContainsKey(partialPath)) {
                    folders.Add(libraries[partialPath]);
                }
                foreach (string folder in folders) {
                    string file = Utils.JoinPaths(folder, filename);
                    if (Utils.FileExists(file)) yield return file;
                }
            }
        }
    }

    DispatchedFile DispatchPath(string path, string oldCompilerPath=null, SPECFileCompiler oldCompiler=null) {
        string extension = Path.GetExtension(path);
        switch (extension) {
            case ".epslspec":
                return DispatchEPSLSPEC(path, oldCompilerPath, oldCompiler);
            case ".epsl":
                return DispatchEPSL(path, oldCompilerPath, oldCompiler);
            default:
                return null;
        }
    }

    DispatchedFile DispatchPartialPath(string partialPath, string projDirectory, bool canBeSPEC=true) {
        DispatchedFile dispatched = null;
        bool anyLocation = false;
        for (int attempts = 0; dispatched == null && attempts < LOAD_RETRY; attempts++) {
            try {
                anyLocation = false;
                foreach (string location in FileLocations(partialPath, projDirectory)) {
                    if (!canBeSPEC && Path.GetExtension(location) == ".epslspec")
                        continue;
                    dispatched = DispatchPath(location);
                    if (dispatched != null) {
                        anyLocation = true;
                        break;
                    }
                }
                if (!anyLocation) break;
            } catch (InvalidSPECResourceException e) {
                CleanupSPEC(e.GetEPSLSPEC(), e.GetObj());
            }
        }
        if (dispatched == null) {
            if (anyLocation) {
                throw new IOException($"Cannot load module '{partialPath}': too many retries");
            } else {
                throw new ModuleNotFoundException(partialPath);
            }
        }
        return dispatched;
    }
    
    FileTree LoadFile(string partialPath, string projDirectory, bool canBeSPEC=true) {
        if (files.ContainsKey(partialPath)) return files[partialPath];
        DispatchedFile dispatched = DispatchPartialPath(partialPath, projDirectory, canBeSPEC);
        string path = dispatched.Path;
        IFileCompiler fileCompiler = dispatched.Compiler;
        currentFile = path;
        currentText = fileCompiler.GetText();
        FileTree result = new FileTree(
            partialPath, path, fileCompiler, dispatched.OldCompilerPath, dispatched.OldCompiler,
            dispatched.GeneratedSPEC
        );
        files[partialPath] = result;
        return result;
    }

    void RecompileFile(FileTree file, string projDirectory) {
        DispatchedFile dispatched = DispatchPartialPath(file.PartialPath, projDirectory, false);
        file.Compiler = dispatched.Compiler;
        file.Path = dispatched.Path;
        file.Text = file.Compiler.GetText();
        SwitchFile(file);
        file.Compiler.ToImports();
    }

    void LoadTree(FileTree tree, string projDirectory) {
        if (tree.TreeLoaded) return;
        tree.TreeLoaded = true;
        foreach (string path in tree.Imports) {
            FileTree sub = LoadFile(path, projDirectory);
            LoadTree(sub, projDirectory);
            tree.Imported.Add(sub);
        }
    }

    void DeleteUnusedGeneratedSPECs(List<string> lastGeneratedSPECs, List<string> generatedSPECs) {
        foreach (string path in lastGeneratedSPECs) {
            if (generatedSPECs.Contains(path)) continue;
            CleanupSPEC(path);
        }
    }

    void DetermineIsProj() {
        int userFiles = 0;
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            if (file.Compiler.GetFileSourceType() == FileSourceType.User) {
                userFiles++;
                if (userFiles >= 2) {
                    isProj = true;
                    return;
                }
            }
        }
    }

    void TransferStructIDs() {
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            file.StructIDs = file.Compiler.ToStructIDs();
        }
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            foreach (FileTree imported in file.Imported) {
                file.Compiler.AddStructIDs(imported.StructIDs);
            }
        }
    }

    void TransferDeclarations() {
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            file.Declarations = file.Compiler.ToDeclarations();
        }
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            foreach (FileTree imported in file.Imported) {
                file.Compiler.AddDeclarations(imported.Declarations);
            }
        }
    }

    void TransferStructs() {
        structs = new HashSet<Struct>();
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            HashSet<Struct> structsHere = file.Compiler.ToStructs();
            file.Structs = structsHere;
            structs.UnionWith(structsHere);
        }
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            file.Compiler.SetStructs(structs);
        }
    }

    public FileTree GetFile(string path) {
        foreach (FileTree file in files.Values) {
            if (file.Stemmed == path) return file;
        }
        return null;
    }

    void SetupOldCompilers() {
        foreach (FileTree file in files.Values) {
            if (file.OldCompiler == null) continue;
            SwitchToOldFile(file);
            file.OldCompiler.ToStructIDs();
            file.OldCompiler.LoadSPECTypes_();
            file.OldStructs = file.OldCompiler.ToStructs();
            file.OldDeclarations = file.OldCompiler.ToDeclarations();
        }
    }

    void ConfirmDependencies(string projectDirectory) {
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            try {
                file.Dependencies = file.Compiler.ToDependencies(GetFile);
            } catch (RecompilationRequiredException) {
                RecompileFile(file, projectDirectory);
                file.Compiler.ToImports();
                file.Compiler.ToStructIDs();
                foreach (FileTree imported in file.Imported) {
                    file.Compiler.AddStructIDs(imported.StructIDs);
                }
                file.Compiler.ToDeclarations();
                foreach (FileTree imported in file.Imported) {
                    file.Compiler.AddDeclarations(imported.Declarations);
                }
                file.Compiler.ToStructs();
                file.Compiler.SetStructs(structs);
                file.Dependencies = file.Compiler.ToDependencies(GetFile);
            }
        }
    }

    List<string> ToIR() {
        List<string> sections = new List<string>();
        foreach (FileTree file in files.Values) {
            if (!LINK_LIBRARIES && file.SourceType == FileSourceType.Library) {
                unlinkedFiles.Add(file);
            } else {
                SwitchFile(file);
                string directory = Utils.GetDirectoryName(currentFile);
                string filename = "." + file.GetName();
                string path = Utils.JoinPaths(directory, filename);
                string ir = file.Compiler.ToIR(path);
                if (path == Path.ChangeExtension(ir, null)) {
                    IRInUserDir.Add(ir);
                }
                sections.Add(ir);
                file.IR = ir;
            }
        }
        return sections;
    }

    void SaveEPSLSPECs() {
        if (!IsProjMode()) return;
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            if (file.Compiler.ShouldSaveSPEC()) {
                JSONObject spec = file.EPSLSPEC.ToJSON(this);
                string directory = Utils.GetDirectoryName(currentFile);
                string filename = file.GetName();
                string path = Utils.JoinPaths(directory, $".{filename}.epslspec");
                file.GeneratedEPSLSPEC = path;
                currentFile = path;
                BJSONEnv.WriteFile(path, spec);
            }
        }
    }

    IEnumerable<string> GetGeneratedEPSLSPECs() {
        foreach (FileTree file in files.Values) {
            if (file.GeneratedEPSLSPEC != null)
                yield return file.GeneratedEPSLSPEC;
        }
    }

    void ShowCompilationError(SyntaxErrorException e, string text, string file) {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("compilation error in ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(file);
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(": ");
        Console.ResetColor();
        Console.WriteLine(e.Message);

        CodeSpan span = e.span;

        if (span == null || text == null) return;

        int startLine = 1;
        int endLine = 1;
        int totalLines = 1;
        int stage = 0;
        int startIndex = 0;

        List<string> lines = new List<string> {""};

        for (int i = 0; i < text.Length; i++) {
            if (stage == 0 && i == span.GetStart()) {
                stage = 1;
            } else if (stage == 1 && i == span.GetEnd()+1) {
                stage = 2;
            }
            char chr = text[i];
            if (chr == '\n') {
                if (stage == 0) startIndex = 0;
                if (stage <= 0)
                    startLine++;
                if (stage <= 1)
                    endLine++;
                totalLines++;
                lines.Add("");
            } else {
                lines[lines.Count-1] += chr;
                if (stage == 0) startIndex++;
            }
        }

        bool oneLine = startLine == endLine;

        Console.Write(oneLine ? "Line " : "Lines ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(startLine);
        Console.ResetColor();

        if (oneLine) {
            Console.WriteLine();

            string linenum = startLine.ToString();
            string line = lines[startLine-1];
            while (line.Length > 0 && Utils.Whitespace.Contains(line[0])) {
                line = line.Substring(1);
                startIndex--;
            }
            Console.WriteLine(line);
            Console.Write(new string(' ', startIndex));
            Console.ForegroundColor = ConsoleColor.Green;
            if (span.Size() == 1) {
                Console.Write("^");
            } else {
                Console.Write("┗");
                for (int i = 0; i < span.Size()-2; i++)
                    Console.Write("━");
                Console.Write("┛");
            }
            Console.ResetColor();
            Console.WriteLine();
        } else {
            Console.Write("–");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(endLine);
            Console.ResetColor();

            int firstLine = Math.Max(1, startLine-1);
            int lastLine = Math.Min(lines.Count, endLine+1);

            int prefixLen = lastLine.ToString().Length + 1;

            for (int line = firstLine; line <= lastLine; line++) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(line.ToString().PadRight(prefixLen));
                Console.ResetColor();
                string prefix = "  ";
                if (line == startLine) {
                    prefix = "┏╸";
                } else if (line == endLine) {
                    prefix = "┗╸";
                } else if (line > startLine && line < endLine) {
                    prefix = "┃ ";
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(prefix);
                Console.ResetColor();
                Console.WriteLine(lines[line-1]);
            }
        }
    }

    public ResultStatus ToExecutable(BuildInfo buildInfo) {
        return RunWrapped(() => {
            if (buildInfo.FileWithMain == null) {
                throw new ProjectProblemException("One main function is required when creating an executable; no main function found");
            }
            Log.Status("Optimizing LLVM");
            Utils.RunCommand("opt", new List<string> {
                "-O3", "-o", Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-opt.bc"),
                Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-linked.bc")
            });
            Log.Status("Buiding executable");
            IEnumerable<string> clangFlags = buildInfo.ClangConfig
                .Select(config => config.Stringify());
            Utils.RunCommand("clang", clangFlags.Concat(new List<string> {
                "-lc", "-lm", "-o", Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code"),
                Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-opt.bc"),
            }));
        });
    }

    public ResultStatus Teardown(EPSLPROJ proj) {
        return RunWrapped(() => {
            Log.Status("Cleaning up EPSLSPECs");
            foreach (string spec in proj.EPSLSPECS) {
                CleanupSPEC(spec);
            }

            Log.Status("Deleting project file");
            Utils.TryDelete(proj.Path);
        });
    }

    public ResultStatus CreateEPSLPROJ(string path) {
        return RunWrapped(() => {
            EPSLPROJ proj = new EPSLPROJ(Utils.GetFullPath(path));
            proj.ToFile();
        });
    }
    
    public IJSONValue ParseJSONFile(string path) {
        return ParseJSONFile(path, out string _);
    }

    public IJSONValue ParseJSONFile(string path, out string fileText) {
        using (FileStream file = new FileStream(path, FileMode.Open)) {
            BinaryReader bytes = new BinaryReader(file);
            if (bytes.PeekChar() == 0x42) {
                fileText = null;
                return BJSONEnv.Deserialize(bytes);
            }
        }

        using (StreamReader file = new StreamReader(path)) {
            fileText = file.ReadToEnd();
        }
        currentText = fileText;
        return JSONTools.ParseJSON(fileText);
    }
}

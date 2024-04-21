using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

public class Builder {
    public bool ALWAYS_PROJECT = false;
    public bool NEVER_PROJECT = false;
    
    bool isProj = false;
    string currentFile = "";
    string currentText = "";
    long? lastBuildStartTime;
    Dictionary<string, FileTree> files;
    List<string> extensions = new List<string> {"epslspec", "epsl"};
    List<string> prefixes = new List<string> {"", "."};
    List<string> IRInUserDir;

    public CompilationResult Build(string input) {
        return RunWrapped(() => {
            long buildStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string projectDirectory = Utils.GetFullPath(Utils.GetDirectoryName(input));

            string entrypoint = Utils.GetFileNameWithoutExtension(input);
            string projName = entrypoint+".epslproj";
            string projLocation = Utils.JoinPaths(projectDirectory, projName);
            EPSLPROJ proj = null;
            lastBuildStartTime = null;
            List<string> lastGeneratedSPECs = new List<string>();
            if (!NEVER_PROJECT && Utils.FileExists(projLocation)) {
                string fileText;
                currentFile = projLocation;
                using (StreamReader file = new StreamReader(projLocation)) {
                    fileText = file.ReadToEnd();
                }
                currentText = fileText;
                proj = EPSLPROJ.FromText(fileText);
                lastBuildStartTime = proj.CompileStartTime;
                lastGeneratedSPECs = proj.EPSLSPECS;
                isProj = true;
            }

            currentFile = Utils.GetFullPath(Utils.RemoveExtension(input));
            files = new Dictionary<string, FileTree>();
            FileTree tree = LoadFile(entrypoint, projectDirectory);

            LoadTree(tree, projectDirectory);
            DetermineIsProj();
            TransferStructIDs();
            TransferStructs();
            TransferDeclarations();
            IRInUserDir = new List<string>();
            List<string> sections = ToIR();
            SaveEPSLSPECS();

            List<string> generatedSPECs = GetGeneratedEPSLSPECs().ToList();
            DeleteUnusedGeneratedSPECs(lastGeneratedSPECs, generatedSPECs);
            
            string builtins = Utils.JoinPaths(Utils.ProjectAbsolutePath(), "builtins.bc");
            List<string> arguments = new List<string> {
                "-o", Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-linked.bc"), "--", builtins
            };
            arguments.AddRange(sections);
            Utils.RunCommand("llvm-link", arguments);

            if (!IsProjMode()) {
                foreach (string path in IRInUserDir) {
                    Utils.TryDelete(path);
                }
            }

            if (IsProjMode()) {
                EPSLPROJ newProj = new EPSLPROJ(buildStartTime, generatedSPECs);
                newProj.ToFile(projLocation);
            }
        });
    }

    bool IsProjMode() {
        return !NEVER_PROJECT && (ALWAYS_PROJECT || isProj);
    }
    
    public CompilationResult RunWrapped(Action action) {
        try {
            action();
        } catch (SyntaxErrorException e) {
            ShowCompilationError(e, currentText, currentFile);
            return new CompilationResult(CompilationResultStatus.USERERR);
        } catch (TargetInvocationException e) {
            Exception inner = e.InnerException;
            if (inner is SyntaxErrorException) {
                ShowCompilationError(
                    (SyntaxErrorException)inner, currentText, currentFile
                );
                return new CompilationResult(CompilationResultStatus.USERERR);
            } else {
                ExceptionDispatchInfo.Capture(inner).Throw();
                // The next line won't be called, we just need it to keep the
                // C# compiler happy
                return new CompilationResult(CompilationResultStatus.FAIL);
            }
        } catch (CommandFailureException e) {
            Console.WriteLine(e.Message);
            return new CompilationResult(CompilationResultStatus.FAIL);
        } catch (FileNotFoundException e) {
            Console.Write(e.Message);
            if (e.FileName != null) {
                Console.Write(": " + e.FileName);
            }
            Console.WriteLine();
            return new CompilationResult(CompilationResultStatus.USERERR);
        } catch (IOException e) {
            return new CompilationResult(
                CompilationResultStatus.USERERR, 
                $"{e.Message}: {currentFile}"
            );
        } catch (InvalidJSONException e) {
            JSONTools.ShowError(currentText, e, currentFile);
            return new CompilationResult(CompilationResultStatus.USERERR);
        } catch (ModuleNotFoundException e) {
            Console.WriteLine($"Cannot find requested module '{e.Path}'");
            return new CompilationResult(CompilationResultStatus.USERERR);
        }
        
        return new CompilationResult(CompilationResultStatus.GOOD);
    }

    void SwitchFile(FileTree file) {
        currentFile = file.File;
        currentText = file.Text;
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
        string fileText;
        using (StreamReader file = new StreamReader(path)) {
            fileText = file.ReadToEnd();
        }
        currentText = fileText;
        IJSONValue jsonValue = JSONTools.ParseJSON(fileText);
        ShapedJSON obj = new ShapedJSON(jsonValue, SPECFileCompiler.Shape);
        CleanupSPEC(path, obj);
    }

    int LOAD_RETRY = 3;

    public DispatchedFile DispatchEPSLSPEC(string path) {
        string fileText;
        currentFile = path;
        using (StreamReader file = new StreamReader(path)) {
            fileText = file.ReadToEnd();
        }
        currentText = fileText;
        IJSONValue jsonValue = JSONTools.ParseJSON(fileText);
        ShapedJSON obj = new ShapedJSON(jsonValue, SPECFileCompiler.Shape);
        string source = Utils.GetFullPath(obj["source"].GetString());
        string generatedEPSLSPEC = null;
        if (source != null) {
            if (NEVER_PROJECT) return null;
            if (!Utils.FileExists(source)) {
                throw new InvalidSPECResourceException(
                    obj, path, source
                );
            }
            generatedEPSLSPEC = path;
            currentFile = source;
            if (lastBuildStartTime == null) return DispatchFile(source);
            long fileModified = new DateTimeOffset(File.GetLastWriteTime(source)).ToUnixTimeSeconds();
            if (fileModified > lastBuildStartTime.Value) return DispatchFile(source);
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
            new SPECFileCompiler(path, fileText, obj), path, generatedEPSLSPEC
        );
    }

    public DispatchedFile DispatchEPSL(string path) {
        return new DispatchedFile(new EPSLFileCompiler(path), path);
    }

    IEnumerable<string> FileLocations(string path, string projDirectory) {
        foreach (string extension in extensions) {
            foreach (string prefix in prefixes) {
                string file = prefix + path + "." + extension;
                currentFile = file;
                string project = Utils.JoinPaths(projDirectory, file);
                if (Utils.FileExists(project)) yield return project;
                string lib = Utils.JoinPaths(Utils.ProjectAbsolutePath(), "libs", file);
                if (Utils.FileExists(lib)) yield return lib;
            }
        }
    }

    DispatchedFile DispatchFile(string path) {
        string extension = path.Substring(path.LastIndexOf('.')+1);
        switch (extension) {
            case "epslspec":
                return DispatchEPSLSPEC(path);
            case "epsl":
                return DispatchEPSL(path);
            default:
                return null;
        }
    }
    
    FileTree LoadFile(string partialPath, string projDirectory) {
        if (files.ContainsKey(partialPath)) return files[partialPath];
        DispatchedFile dispatched = null;
        bool anyLocation = false;
        for (int attempts = 0; dispatched == null && attempts < LOAD_RETRY; attempts++) {
            try {
                anyLocation = false;
                foreach (string location in FileLocations(partialPath, projDirectory)) {
                    dispatched = DispatchFile(location);
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
        string path = dispatched.Path;
        IFileCompiler fileCompiler = dispatched.Compiler;
        currentFile = path;
        currentText = fileCompiler.GetText();
        FileTree result = new FileTree(
            path, fileCompiler, dispatched.GeneratedSPEC
        );
        files[partialPath] = result;
        return result;
    }

    void LoadTree(FileTree tree, string projDirectory) {
        if (tree.TreeLoaded) return;
        tree.TreeLoaded = true;
        foreach (string path in tree.Imports) {
            FileTree sub = LoadFile(path, projDirectory);
            LoadTree(sub, projDirectory);
            tree.Dependencies.Add(sub);
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
            foreach (FileTree dependency in file.Dependencies) {
                file.Compiler.AddStructIDs(dependency.StructIDs);
            }
        }
    }

    void TransferStructs() {
        HashSet<Struct> structs = new HashSet<Struct>();
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            HashSet<Struct> structsHere = file.Compiler.ToStructs();
            file.Structs = structsHere;
            structs.UnionWith(structsHere);
        }
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            file.Compiler.AddStructs(structs);
        }
    }

    void TransferDeclarations() {
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            file.Declarations = file.Compiler.ToDeclarations();
        }
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            foreach (FileTree dependency in file.Dependencies) {
                file.Compiler.AddDeclarations(dependency.Declarations);
            }
        }
    }

    List<string> ToIR() {
        List<string> sections = new List<string>();
        foreach (FileTree file in files.Values) {
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
        return sections;
    }

    void SaveEPSLSPECS() {
        if (!IsProjMode()) return;
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            if (file.Compiler.ShouldSaveSPEC()) {
                JSONObject spec = file.ToSPEC();
                string directory = Utils.GetDirectoryName(currentFile);
                string filename = file.GetName();
                string path = Utils.JoinPaths(directory, $".{filename}.epslspec");
                file.GeneratedEPSLSPEC = path;
                using (StreamWriter writer = new StreamWriter(path)) {
                    writer.Write(spec.ToJSON());
                }
            }
        }
    }

    IEnumerable<string> GetGeneratedEPSLSPECs() {
        foreach (FileTree file in files.Values) {
            if (file.GeneratedEPSLSPEC != null)
                yield return file.GeneratedEPSLSPEC;
        }
    }

    IEnumerable<string> GetClangFlags() {
        IEnumerable<IClangConfig> configs = files.Values.SelectMany(
            file => file.Compiler.GetClangConfig());
        return configs.Select(config => config.Stringify()).Distinct();
    }

    void ShowCompilationError(SyntaxErrorException e, string text, string file) {
        CodeSpan span = e.span;

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

        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("compilation error in ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(file);
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(": ");
        Console.ResetColor();
        Console.WriteLine(e.Message);

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

    public CompilationResult ToExecutable() {
        return RunWrapped(() => {
            Utils.RunCommand("opt", new List<string> {
                "-O3", "-o", Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-opt.bc"),
                Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-linked.bc")
            });
            Utils.RunCommand("clang", GetClangFlags().Concat(new List<string> {
                "-lc", "-lm", "-o", Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code"),
                Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-opt.bc"),
            }));
        });
    }

    public CompilationResult Teardown(string projFile) {
        return RunWrapped(() => {
            string projectDirectory = Utils.GetFullPath(Utils.GetDirectoryName(projFile));

            string fileText;
            currentFile = projFile;
            using (StreamReader file = new StreamReader(projFile)) {
                fileText = file.ReadToEnd();
            }
            currentText = fileText;
            EPSLPROJ proj = EPSLPROJ.FromText(fileText);

            foreach (string spec in proj.EPSLSPECS) {
                CleanupSPEC(spec);
            }

            Utils.TryDelete(projFile);
        });
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

public class Builder {
    string currentFile = "";
    string currentText = "";
    long? lastBuildStartTime;
    Dictionary<string, FileTree> files;
    List<string> extentions = new List<string> {"epslspec", "epsl"};
    List<string> prefixes = new List<string> {"", "."};

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
            if (Utils.FileExists(projLocation)) {
                string fileText;
                currentFile = projLocation;
                using (StreamReader file = new StreamReader(projLocation)) {
                    fileText = file.ReadToEnd();
                }
                currentText = fileText;
                proj = EPSLPROJ.FromText(fileText);
                lastBuildStartTime = proj.CompileStartTime;
                lastGeneratedSPECs = proj.EPSLSPECS;
            }

            currentFile = Utils.GetFullPath(Utils.RemoveExtention(input));
            files = new Dictionary<string, FileTree>();
            FileTree tree = LoadFile(entrypoint, projectDirectory);

            LoadTree(tree, projectDirectory);
            TransferStructIDs();
            TransferStructs();
            TransferDeclarations();
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

            EPSLPROJ newProj = new EPSLPROJ(buildStartTime, generatedSPECs);
            newProj.ToFile(projLocation);
        });
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

    DispatchedFile DispatchByExtention(string path) {
        string extention = path.Substring(path.LastIndexOf('.')+1);
        currentFile = path;
        if (extention == "epsl") {
            return new DispatchedFile(new CodeFileCompiler(path), path);
        }
        throw new IOException($"Invalid extention for source file '{extention}'");
    }

    DispatchedFile DispatchFile(string path) {
        string extention = path.Substring(path.LastIndexOf('.')+1);
        if (extention != "epslspec") {
            return DispatchByExtention(path);
        }
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
            if (!Utils.FileExists(source)) {
                throw new InvalidSPECResourceException(
                    obj, path, source
                );
            }
            generatedEPSLSPEC = path;
            currentFile = source;
            long fileModified = new DateTimeOffset(File.GetLastWriteTime(source)).ToUnixTimeSeconds();
            if (lastBuildStartTime == null || fileModified > lastBuildStartTime.Value) {
                return DispatchByExtention(source);
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
            new SPECFileCompiler(path, fileText, obj), path, generatedEPSLSPEC
        );
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

    int LOAD_RETRY = 3;

    FileTree LoadFile(string partialPath, string projDirectory) {
        string path;
        DispatchedFile dispatched = null;
        for (int i = 0; dispatched == null && i < LOAD_RETRY; i++) {
            try {
                currentFile = partialPath;
                path = Utils.GetFullPath(FindFile(partialPath, projDirectory));
                if (path == null) throw new ModuleNotFoundException(partialPath);
                currentFile = path;
                if (files.ContainsKey(path)) return files[path];
                dispatched = DispatchFile(path);
            } catch (InvalidSPECResourceException e) {
                CleanupSPEC(e.GetEPSLSPEC(), e.GetObj());
            }
        }
        if (dispatched == null) {
            throw new IOException($"Cannot load module '{partialPath}': to many retries");
        }
        path = dispatched.Path;
        IFileCompiler fileCompiler = dispatched.Compiler;
        currentFile = path;
        currentText = fileCompiler.GetText();
        FileTree result = new FileTree(
            path, fileCompiler, dispatched.GeneratedSPEC
        );
        files[path] = result;
        return result;
    }

    string FindFile(string path, string projDirectory) {
        foreach (string extention in extentions) {
            foreach (string prefix in prefixes) {
                string file = prefix + path + "." + extention;
                currentFile = file;
                string project = Utils.JoinPaths(projDirectory, file);
                if (Utils.FileExists(project)) return project;
                string lib = Utils.JoinPaths(Utils.ProjectAbsolutePath(), "libs", file);
                if (Utils.FileExists(lib)) return lib;
            }
        }
        return null;
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
            string fileText;
            using (StreamReader file = new StreamReader(path)) {
                fileText = file.ReadToEnd();
            }
            currentText = fileText;
            IJSONValue jsonValue = JSONTools.ParseJSON(fileText);
            ShapedJSON obj = new ShapedJSON(jsonValue, SPECFileCompiler.Shape);
            CleanupSPEC(path, obj);
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
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            file.Structs = file.Compiler.ToStructs();
        }
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            foreach (FileTree dependency in file.Dependencies) {
                file.Compiler.AddStructs(dependency.Structs);
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
            string filename = file.GetName();
            string path = Utils.JoinPaths(directory, "." + filename);
            string ir = file.Compiler.ToIR(path);
            sections.Add(ir);
            file.IR = ir;
        }
        return sections;
    }

    void SaveEPSLSPECS() {
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
            Utils.RunCommand("bash", new List<string> {
                "--", Utils.JoinPaths(Utils.ProjectAbsolutePath(), "compileir.bash")
            });
        });
    }
}

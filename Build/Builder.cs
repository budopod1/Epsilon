using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

public class Builder {
    string projectDirectory = "";
    string currentFile = "";
    string currentText = "";
    List<string> sections;

    public CompilationResult Build(string input) {
        return RunWrapped(() => {
            projectDirectory = Path.GetDirectoryName(input);
            FileTree tree = LoadFile(input, true);
            LoadTree(tree);
            TransferStructIDs(tree);
            TransferStructs(tree);
            TransferDeclarations(tree);
            sections = new List<string>();
            int i = 0;
            foreach (FileTree file in tree.IterTree()) {
                currentFile = file.File;
                currentText = file.Compiler.GetText();
                sections.Add(file.Compiler.ToExecutable(Path.Combine(
                    Utils.ProjectAbsolutePath(), "sections", $"section{i}"
                )));
                i++;
            }
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
        } catch (IOException) {
            return new CompilationResult(
                CompilationResultStatus.USERERR, 
                $"Cannot read {currentFile}"
            );
        } catch (InvalidJSONException e) {
            JSONTools.ShowError(currentText, e, currentFile);
            return new CompilationResult(CompilationResultStatus.USERERR);
        } catch (FileNotFoundErrorException e) {
            Console.WriteLine($"Cannot find requested file or module {e.Path}");
            return new CompilationResult(CompilationResultStatus.USERERR);
        }
        
        return new CompilationResult(CompilationResultStatus.GOOD);
    }

    FileTree LoadFile(string partialPath, bool directPath = false) {
        string path = directPath ? Path.GetFullPath(partialPath) : FindFile(partialPath);
        if (path == null) throw new FileNotFoundErrorException(partialPath);
        string extention = path.Substring(path.LastIndexOf('.')+1);
        IFileCompiler fileCompiler;
        if (extention == "epsl") {
            fileCompiler = new CodeFileCompiler(path);
        } else if (extention == "epslspec") {
            fileCompiler = new SPECFileCompiler(path);
        } else {
            return null;
        }
        currentFile = path;
        currentText = fileCompiler.GetText();
        return new FileTree(partialPath, fileCompiler, fileCompiler.ToImports());
    }

    string FindFile(string path) {
        foreach (string extention in new string[] {".epsl", ".epslspec"}) {
            string file = path + extention;
            string project = Path.Combine(projectDirectory, file);
            if (Utils.FileExists(project)) return project;
            string lib = Path.Combine(Utils.ProjectAbsolutePath(), "libs", file);
            if (Utils.FileExists(lib)) return lib;
        }
        return null;
    }

    void LoadTree(FileTree tree) {
        foreach (string path in tree.DependencyPaths) {
            FileTree sub = LoadFile(path);
            LoadTree(sub);
            tree.Dependencies.Add(sub);
        }
    }

    HashSet<LocatedID> TransferStructIDs(FileTree tree) {
        HashSet<LocatedID> baseTypes_ = new HashSet<LocatedID>();
        foreach (FileTree dependency in tree.Dependencies) {
            baseTypes_.UnionWith(TransferStructIDs(dependency));
        }
        currentFile = tree.File;
        currentText = tree.Compiler.GetText();
        HashSet<LocatedID> hereTypes_ = tree.Compiler.ToStructIDs();
        tree.Compiler.AddStructIDs(baseTypes_);
        baseTypes_.UnionWith(hereTypes_);
        return baseTypes_;
    }

    List<Struct> TransferStructs(FileTree tree) {
        List<Struct> structs = new List<Struct>();
        foreach (FileTree dependency in tree.Dependencies) {
            structs.AddRange(TransferStructs(dependency));
        }
        currentFile = tree.File;
        currentText = tree.Compiler.GetText();
        List<Struct> hereStructs = tree.Compiler.ToStructs();
        tree.Compiler.AddStructs(structs);
        structs.AddRange(hereStructs);
        return structs;
    }

    List<RealFunctionDeclaration> TransferDeclarations(FileTree tree) {
        List<RealFunctionDeclaration> declarations = new List<RealFunctionDeclaration>();
        foreach (FileTree dependency in tree.Dependencies) {
            declarations.AddRange(TransferDeclarations(dependency));
        }
        currentFile = tree.File;
        currentText = tree.Compiler.GetText();
        List<RealFunctionDeclaration> hereDeclarations = tree.Compiler.ToDeclarations();
        tree.Compiler.AddDeclarations(declarations);
        declarations.AddRange(hereDeclarations);
        return declarations;
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
            string builtins = Path.Combine(Utils.ProjectAbsolutePath(), "builtins.bc");
            List<string> arguments = new List<string> {
                "-o", $"{Utils.ProjectAbsolutePath()}/code-linked.bc", "--", builtins
            };
            arguments.AddRange(sections);
            Utils.RunCommand("llvm-link", arguments);
            Utils.RunCommand("bash", new List<string> {
                "--", Path.Combine(Utils.ProjectAbsolutePath(), "compileir.bash")
            });
        });
    }
}

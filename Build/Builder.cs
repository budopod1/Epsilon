using CsCommandConfig;
using CsJSONTools;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Epsilon;
public class Builder {
    static readonly Dictionary<string, Func<BuildSettings, string, IFileCompiler>> dispatchers = [];
    public static IJSONShape EPSLPROJShape;

    bool shouldCache = false;
    string currentFile = "";
    string currentText = "";
    Dictionary<string, string> packagePaths = [];
    Dictionary<string, FileTree> files;
    static readonly List<string> EXTENSIONS = ["epslspec"];
    static readonly List<string> PREFIXES = ["", "."];

    public static void RegisterDispatcher(Func<BuildSettings, string, IFileCompiler> dispatcher, params string[] extensions) {
        EXTENSIONS.AddRange(extensions);
        foreach (string extension in extensions) {
            dispatchers[extension] = dispatcher;
        }
    }

    static Builder() {
        EPSLFileCompiler.Setup();
        HeaderFileCompiler.Setup();
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
            Console.WriteLine("IO Error: " + e.Message);
            return ResultStatus.USERERR;
        } catch (UnauthorizedAccessException e) {
            Console.WriteLine(e.Message);
            return ResultStatus.FAIL;
        } catch (HttpRequestException e) {
            if (e.StatusCode != null) {
                Console.WriteLine($"Got HTTP error code {e.StatusCode}");
            }
            Console.WriteLine(e.Message);
            return ResultStatus.FAIL;
        } catch (UriFormatException e) {
            Console.WriteLine(e.Message);
            return ResultStatus.USERERR;
        } catch (InvalidJSONException e) {
            JSONTools.ShowError(currentText, e, currentFile);
            return ResultStatus.USERERR;
        } catch (InvalidBinJSONException e) {
            Console.WriteLine($"Error while reading BinJSON file {currentFile}: {e.Message}");
            return ResultStatus.FAIL;
        } catch (ModuleNotFoundException e) {
            Console.WriteLine($"Cannot find requested module '{e.Path}'");
            return ResultStatus.USERERR;
        } catch (ProjectProblemException e) {
            Console.WriteLine(e.Message);
            return ResultStatus.USERERR;
        } catch (PackagingProblemException e) {
            if (e.Package_ != null) {
                Console.WriteLine($"With respect to the package {e.Package_.Name}:");
            }
            Console.WriteLine(e.Message);
            return ResultStatus.USERERR;
        } catch (FileProblemException e) {
            Console.WriteLine($"In {currentFile}:");
            Console.WriteLine(e.Message);
            return ResultStatus.USERERR;
        }

        return ResultStatus.GOOD;
    }

    public ResultStatus WipeTempDir() {
        return RunWrapped(() => {
            Directory.Delete(Utils.TempDir(), recursive: true);
            Directory.CreateDirectory(Utils.TempDir());
        });
    }

    public ResultStatus ComputeInputPath(string baseInput, out string input) {
        string _input = baseInput ?? "";
        ResultStatus result = RunWrapped(() => {
            if (!Path.IsPathRooted(_input))
                _input = $".{Path.DirectorySeparatorChar}{_input}";
            _input = Utils.GetFullPath(_input);
            if (Utils.GetFileName(_input) == "") {
                _input = Utils.JoinPaths(_input, "entry");
            }
        });
        input = _input;
        return result;
    }

    public ResultStatus LoadEPSLPROJ(string input, JSONConfigParser EPSLPROJParser, bool allowNew=true) {
        string projLocation = null;
        ResultStatus status1 = RunWrapped(() => {
            string projDirectory = Utils.GetFullPath(Utils.GetDirectoryName(input));
            string projName = Utils.SetExtension(input, "epslproj");
            projLocation = Utils.JoinPaths(projDirectory, projName);
        });

        if (status1 != ResultStatus.GOOD) {
            return status1;
        }

        bool projExists = Utils.FileExists(projLocation);

        if (!allowNew && !projExists) {
            Console.WriteLine("Cannot find requested EPSLPROJ");
            return ResultStatus.USERERR;
        }

        return RunWrapped(() => {
            if (projExists) {
                currentFile = projLocation;
                EPSLPROJParser.Parse(projLocation, text => currentText = text);
                Log.Info("Loaded EPSLPROJ");
            }
        });
    }

    public ResultStatus LoadEPSLCACHE(string input, CacheMode cacheMode, out EPSLCACHE cacheOut) {
        EPSLCACHE cache = null;

        string cacheLocation = null;
        ResultStatus status1 = RunWrapped(() => {
            string cacheDirectory = Utils.GetFullPath(Utils.GetDirectoryName(input));
            string cacheName = $".{Utils.GetFileNameWithoutExtension(input)}.epslcache";
            cacheLocation = Utils.JoinPaths(cacheDirectory, cacheName);
        });
        if (status1 != ResultStatus.GOOD) {
            cacheOut = null;
            return status1;
        }

        // We can still load the .epslcache in DONTLOAD mode, because the EPSLSPEC files referenced
        // by it won't be used anyway
        if (cacheMode <= CacheMode.DONTUSE) {
            cacheOut = new EPSLCACHE(cacheLocation);
            return status1;
        }

        ResultStatus status2 = RunWrapped(() => {
            if (Utils.FileExists(cacheLocation)) {
                currentFile = cacheLocation;
                cache = EPSLCACHE.FromText(cacheLocation, ParseJSONFile(cacheLocation));
                Log.Info("Loaded EPSLCACHE");
            }
        });

        cacheOut = cache ?? new EPSLCACHE(cacheLocation);

        return status2;
    }

    void ReadyPackageFolder(string folder) {
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
    }

    public ResultStatus LoadPackagesForCompilation(IEnumerable<string> packageNames) {
        return RunWrapped(() => {
            IEnumerable<Package> packages = ReadPackageJSON();
            Stack<string> packageQueue = new(packageNames);
            while (packageQueue.Count != 0) {
                string packageName = packageQueue.Pop();
                Package package = packages
                    .FirstOrDefault(package => package.Name == packageName);
                if (package == null) {
                    throw new ProjectProblemException(
                        $"No package named '{packageName}' can be found"
                    );
                }
                packagePaths[packageName] = Utils.JoinPaths(package.Path, packageName);
                foreach (string dependency in package.Dependencies) {
                    if (!packagePaths.ContainsKey(dependency)) {
                        packageQueue.Push(dependency);
                    }
                }
            }
        });
    }

    public ResultStatus Build(BuildSettings settings) {
        Log.Info("Starting build of", settings.InputPath);

        return RunWrapped(() => {
            string defaultOutput = GetOutputLocation(settings, out string outputName);
            string output = Utils.GetFullPath(settings.ProvidedOutput ?? defaultOutput);

            long buildStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            EPSLCACHE cache = settings.Cache;

            string input = Utils.GetFullPath(settings.InputPath);
            string projectDirectory = Utils.GetDirectoryName(input);

            List<string> lastGeneratedSPECs = cache.EPSLSPECS;
            shouldCache |= cache.IsFromFile;

            currentFile = input;
            files = [];
            FileTree tree = LoadFile(settings, Utils.GetFileNameWithoutExtension(input),
                projectDirectory);

            Log.Status("Loading tree");
            LoadTree(settings, tree, projectDirectory);
            DetermineShouldCache();
            LoadFileSubconfigs();
            Log.Status("Transfering struct IDs");
            TransferStructIDs();
            Log.Status("Transfering declarations");
            TransferDeclarations();
            Log.Status("Transfering structs");
            TransferStructs();
            Log.Status("Loading struct extendees");
            LoadStructExtendees();
            SetupOldCompilers();
            Log.Status("Confirming dependencies");
            ConfirmDependencies(settings, projectDirectory);
            Log.Status("Finishing individual file compilation");
            FinishCompilations(settings, out List<FileTree> unlinkedFiles);
            Log.Status("Getting intermediates");
            ToIntermediates(settings);

            if (doesSaveCache(settings)) {
                if (!shouldGetIR(settings)) {
                    Log.Status("Creating .o files for caching");
                    CreateCachedObjs(settings);
                }

                Log.Status("Saving EPSLSPECs");
                SaveEPSLSPECs();
            }

            List<string> generatedSPECs = GetGeneratedEPSLSPECs().ToList();
            DeleteUnusedGeneratedSPECs(lastGeneratedSPECs, generatedSPECs);

            string fileWithMain = GetFileWithMain();

            Log.Status("Producing final sources");
            List<string> sources = ProduceFinalSources(settings);

            if (doesSaveCache(settings)) {
                cache.CompileStartTime = buildStartTime;
                cache.EPSLSPECS = generatedSPECs;
                cache.LastOutputType = settings.Output_Type;
                cache.ToFile();
                Log.Info("Saved updated EPSLCACHE");
            }

            BuildInfo buildInfo = new(
                output, outputName, sources, tree, unlinkedFiles, fileWithMain
            );

            Log.Status("Producing final result");

            switch (settings.Output_Type) {
            case OutputType.EXECUTABLE:
                ToExecutable(buildInfo);
                break;
            case OutputType.LLVMLL:
                ToLLVM(buildInfo, toLL: true);
                break;
            case OutputType.LLVMBC:
                ToLLVM(buildInfo, toLL: false);
                break;
            case OutputType.PACKAGEBOTH:
                ToPackage(buildInfo, includeLLVMInPackage: true);
                break;
            case OutputType.PACKAGEOBJ:
                ToPackage(buildInfo, includeLLVMInPackage: false);
                break;
            case OutputType.OBJECT:
                ToObject(buildInfo);
                break;
            case OutputType.SHAREDOBJECT:
                ToSharedObject(buildInfo);
                break;
            default:
                throw new InvalidOperationException();
            }

            Log.Status("Deleting unwanted files in user dir");
            DeleteUnwantedIntermediaries();
        });
    }

    bool doesSaveCache(BuildSettings settings) {
        return settings.CacheMode switch {
            CacheMode.DONTUSE => false,
            CacheMode.DONTLOAD or CacheMode.AUTO => shouldCache,
            CacheMode.ALWAYS => true,
            _ => throw new InvalidOperationException(),
        };
    }

    void SwitchFile(FileTree file) {
        currentFile = file.Path_;
        currentText = file.Text;
    }

    void SwitchToOldFile(FileTree file) {
        currentFile = file.OldPath;
        currentText = file.OldText;
    }

    public string GetOutputLocation(BuildSettings settings, out string outputName) {
        string outputDir = Utils.GetFullPath(Utils.GetDirectoryName(settings.InputPath));
        outputName = Utils.RemoveExtension(Utils.GetFileName(settings.InputPath));
        if (outputName == "entry") outputName = "result";
        string extension = settings.Output_Type.GetExtension();
        string outputFile = Utils.SetExtension(outputName, extension);
        return Utils.JoinPaths(outputDir, outputFile);
    }

    void CleanupSPEC(string path, ShapedJSON obj) {
        string source = obj["source"].GetString();
        foreach (string field in new List<string> {"ir", "obj"}) {
            string fieldPath = obj[field].GetStringOrNull();
            if (fieldPath == null) continue;
            string fullFieldPath = Utils.JoinPaths(
                Utils.GetDirectoryName(path), fieldPath);
            if (source != fullFieldPath) Utils.TryDelete(fullFieldPath);
        }
        Utils.TryDelete(path);
    }

    void CleanupSPEC(string path) {
        if (!File.Exists(path)) return;
        currentFile = path;
        IJSONValue jsonValue = ParseJSONFile(path);
        ShapedJSON obj = new(jsonValue, EPSLSPEC.Shape);
        CleanupSPEC(path, obj);
    }

    readonly int LOAD_RETRY = 3;

    void VerifySPECDependencies(string path, ShapedJSON json) {
        foreach (string field in new List<string> {"ir", "obj"}) {
            string fieldPath = json[field].GetStringOrNull();
            if (fieldPath == null) continue;
            string fullFieldPath = Utils.JoinPaths(
                Utils.GetDirectoryName(path), fieldPath);
            if (Utils.FileExists(fullFieldPath)) continue;
            Log.Info("Rejecting EPSLSPEC file,", field, "dependency could not be found");
            throw new InvalidSPECResourceException(
                json, path, fullFieldPath
            );
        }
    }

    DispatchedFile DispatchEPSLSPEC(BuildSettings settings, string path) {
        currentFile = path;
        string stemmed = Utils.Stem(path);
        string fileText = null;
        IJSONValue jsonValue = JSONTools.ParseJSONFile(path, text => {
            fileText = text;
            currentText = text;
        });
        ShapedJSON obj = new(jsonValue, EPSLSPEC.Shape);
        string source = Utils.GetFullPath(obj["source"].GetStringOrNull());
        string generatedEPSLSPEC = null;
        if (source != null) {
            if (settings.CacheMode <= CacheMode.DONTLOAD) return null;
            if (shouldGetIR(settings) && obj["ir"].IsNull()) return null;
            if (!Utils.FileExists(source)) {
                Log.Info("Rejecting EPSLSPEC file, source file could not be found");
                throw new InvalidSPECResourceException(
                    obj, path, source
                );
            }
            generatedEPSLSPEC = path;
            currentFile = source;
            long? lastBuildStartTime = settings.Cache.CompileStartTime;
            if (lastBuildStartTime == null) {
                return DispatchPath(settings, source, path, new SPECFileCompiler(
                    stemmed, fileText, obj
                ));
            }
            long fileModified = new DateTimeOffset(File.GetLastWriteTime(source)).ToUnixTimeSeconds();
            if (fileModified > lastBuildStartTime.Value) {
                return DispatchPath(settings, source, path, new SPECFileCompiler(
                    stemmed, fileText, obj
                ));
            }
        }
        currentFile = path;
        VerifySPECDependencies(path, obj);
        return new DispatchedFile(
            new SPECFileCompiler(stemmed, fileText, obj), path,
            generatedSPEC: generatedEPSLSPEC
        );
    }

    IEnumerable<string> FileLocations(string partialPath, string projDirectory) {
        foreach (string extension in EXTENSIONS) {
            foreach (string prefix in PREFIXES) {
                string filename = prefix + partialPath + "." + extension;
                currentFile = filename;
                List<string> folders = [
                    projDirectory, Utils.EPSLLIBS()
                ];
                if (packagePaths.TryGetValue(partialPath, out string value)) {
                    folders.Add(value);
                }
                foreach (string folder in folders) {
                    string file = Utils.JoinPaths(folder, filename);
                    if (Utils.FileExists(file)) yield return file;
                }
            }
        }
    }

    DispatchedFile DispatchPath(BuildSettings settings, string path, string oldCompilerPath=null, SPECFileCompiler oldCompiler=null) {
        string extension = Utils.GetExtension(path);
        if (extension == "epslspec") {
            return DispatchEPSLSPEC(settings, path);
        } else if (dispatchers.ContainsKey(extension)) {
            currentFile = path;
            return new DispatchedFile(
                dispatchers[extension](settings, path),
                path, oldCompilerPath, oldCompiler
            );
        } else {
            return null;
        }
    }

    DispatchedFile DispatchPartialPath(BuildSettings settings, string partialPath, string projDirectory, bool canBeSPEC=true) {
        DispatchedFile dispatched = null;
        bool anyLocation = false;
        for (int attempt = 0; dispatched == null && attempt < LOAD_RETRY; attempt++) {
            try {
                anyLocation = false;
                foreach (string location in FileLocations(partialPath, projDirectory)) {
                    if (!canBeSPEC && Utils.GetExtension(location) == "epslspec")
                        continue;
                    dispatched = DispatchPath(settings, location);
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

    FileTree LoadFile(BuildSettings settings, string partialPath, string projDirectory, bool canBeSPEC=true) {
        if (files.ContainsKey(partialPath)) return files[partialPath];
        DispatchedFile dispatched = DispatchPartialPath(settings, partialPath, projDirectory, canBeSPEC);
        string path = dispatched.Path;
        IFileCompiler fileCompiler = dispatched.Compiler;
        currentFile = path;
        currentText = fileCompiler.GetText();
        FileTree result = new(
            partialPath, path, fileCompiler, dispatched.OldCompilerPath, dispatched.OldCompiler,
            dispatched.GeneratedSPEC
        );
        files[partialPath] = result;
        return result;
    }

    void RecompileFile(BuildSettings settings, FileTree file, string projDirectory) {
        DispatchedFile dispatched = DispatchPartialPath(
            settings, file.PartialPath, projDirectory, canBeSPEC: false);
        file.Compiler = dispatched.Compiler;
        file.Path_ = dispatched.Path;
        file.Text = file.Compiler.GetText();
        SwitchFile(file);
        file.Compiler.ToImports();
    }

    void LoadTree(BuildSettings settings, FileTree tree, string projDirectory) {
        if (tree.TreeLoaded) return;
        tree.TreeLoaded = true;
        foreach (string path in tree.Imports) {
            FileTree sub = LoadFile(settings, path, projDirectory);
            LoadTree(settings, sub, projDirectory);
            tree.Imported.Add(sub);
        }
    }

    void DeleteUnusedGeneratedSPECs(List<string> lastGeneratedSPECs, List<string> generatedSPECs) {
        foreach (string path in lastGeneratedSPECs) {
            if (generatedSPECs.Contains(path)) continue;
            CleanupSPEC(path);
        }
    }

    void DetermineShouldCache() {
        int userFiles = 0;
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            if (file.Compiler.GetFileSourceType() == FileSourceType.User) {
                userFiles++;
                if (userFiles >= 2) {
                    shouldCache = true;
                    return;
                }
            }
        }
    }

    void LoadFileSubconfigs() {
        foreach (FileTree file in files.Values) {
            Subconfigs.AddSubconfigCollection(file.Subconfigs);
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
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            HashSet<Struct> structsHere = file.Compiler.ToStructs();
            file.Structs = structsHere;
            StructsCtx.Extend(structsHere);
        }
        StructsCtx.MarkStructsLoaded();
    }

    void LoadStructExtendees() {
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            file.Compiler.LoadStructExtendees();
        }
    }

    public FileTree GetFileByIDPath(string path) {
        foreach (FileTree file in files.Values) {
            if (file.IDPath == path) return file;
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

    void ConfirmDependencies(BuildSettings settings, string projectDirectory) {
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            try {
                file.Dependencies = file.Compiler.ToDependencies(this);
            } catch (RecompilationRequiredException) {
                RecompileFile(settings, file, projectDirectory);
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
                file.Dependencies = file.Compiler.ToDependencies(this);
            }
        }
    }

    bool shouldGetIR(BuildSettings settings) {
        return settings.OptLevel >= OptimizationLevel.MAX || settings.Output_Type.DoesRequireLLVM();
    }

    void FinishCompilations(BuildSettings settings, out List<FileTree> unlinkedFiles) {
        unlinkedFiles = [];
        foreach (FileTree file in files.Values) {
            if ((!settings.LinkLibraries && file.SourceType == FileSourceType.Library)
                || (!settings.LinkBuiltinModules && file.IsBuiltinModule)) {
                file.IsUnlinked = true;
                unlinkedFiles.Add(file);
            } else {
                SwitchFile(file);
                string directory = Utils.GetDirectoryName(currentFile);
                string filename = "." + file.GetName();
                string path = Utils.JoinPaths(directory, filename);
                file.SuggestedIntermediatePath = path;
                file.Compiler.FinishCompilation(path, shouldGetIR(settings));
            }
        }
    }

    void ToIntermediates(BuildSettings settings) {
        foreach (FileTree file in files.Values) {
            if (file.IsUnlinked) continue;
            SwitchFile(file);
            file.IR = file.Compiler.GetIR();
            file.Obj = file.Compiler.GetObj();
            file.Intermediate = IntermediateFile.FromFileTree(file, shouldGetIR(settings));
        }
    }

    void CreateCachedObjs(BuildSettings settings) {
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            IntermediateFile intermediate = file.Intermediate;
            if (intermediate == null) continue;

            if (!(file.Compiler.ShouldSaveSPEC()
                && intermediate.FileType == IntermediateFile.IntermediateType.IR
                && intermediate.IsInUserDir
                && file.Obj == null)) continue;

            string irFile = intermediate.Path;

            if (settings.OptLevel >= OptimizationLevel.NORMAL) {
                string optFile = Utils.JoinPaths(Utils.TempDir(), "opt.bc");
                CmdUtils.OptLLVM(irFile, optFile);
                Utils.TryDelete(irFile);
                irFile = optFile;
            }

            string objFile = file.SuggestedIntermediatePath+".o";
            CmdUtils.LLVMToObj(irFile, objFile);
            Utils.TryDelete(irFile);

            file.IR = null;
            file.Obj = objFile;

            file.Intermediate = new IntermediateFile(
                IntermediateFile.IntermediateType.Obj, objFile, isInUserDir: true);
        }
    }

    void SaveEPSLSPECs() {
        foreach (FileTree file in files.Values) {
            SwitchFile(file);
            if (file.Compiler.ShouldSaveSPEC()) {
                JSONObject spec = file.EPSLSPEC.ToJSON(this);
                string directory = Utils.GetDirectoryName(currentFile);
                string filename = file.GetName();
                string path = Utils.JoinPaths(directory, $".{filename}.epslspec");
                file.GeneratedEPSLSPEC = path;
                currentFile = path;
                BinJSONEnv.WriteFile(path, spec);
            }
        }
    }

    string GetFileWithMain() {
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
        return fileWithMain;
    }

    IEnumerable<string> GetGeneratedEPSLSPECs() {
        foreach (FileTree file in files.Values) {
            if (file.GeneratedEPSLSPEC != null)
                yield return file.GeneratedEPSLSPEC;
        }
    }

    string CombineAndOptimizeLLVM(IEnumerable<IntermediateFile> files) {
        string irLinkedPath = Utils.JoinPaths(Utils.TempDir(), "linked.bc");
        CmdUtils.LinkLLVM(files.Select(intermediate => intermediate.Path), irLinkedPath);

        string irOptedPath = Utils.JoinPaths(Utils.TempDir(), "linked-opt.bc");
        CmdUtils.OptLLVM(irLinkedPath, irOptedPath);

        return irOptedPath;
    }

    IEnumerable<string> OptimizeLLVMIndividually(IEnumerable<IntermediateFile> files) {
        int i = 0;
        foreach (IntermediateFile file in files) {
            string irOptedPath = Utils.JoinPaths(Utils.TempDir(), $"opt-{i++}.bc");
            CmdUtils.OptLLVM(file.Path, irOptedPath);

            yield return irOptedPath;
        }
    }

    void VerifyOnlyLLVMSources(BuildSettings settings, Dictionary<IntermediateFile.IntermediateType, IEnumerable<IntermediateFile>> grouped) {
        foreach (KeyValuePair<IntermediateFile.IntermediateType, IEnumerable<IntermediateFile>> pair in grouped) {
            if (pair.Key == IntermediateFile.IntermediateType.IR) continue;
            IntermediateFile intermediate = pair.Value.FirstOrDefault();
            if (intermediate == null) continue;
            if (settings.LinkLibraries) {
                throw new ProjectProblemException(
                    $"Cannot compile to output type {settings.Output_Type} with source {intermediate.Path}, as it does not have an available LLVM form (which is required for this output type). If this source came from a library, you can compile without libraries linked (with the --no-libraries flag), and then compile and link the library seperately."
                );
            } else {
                throw new ProjectProblemException(
                    $"Cannot compile to output type {settings.Output_Type} with source {intermediate.Path}, as it does not have an available LLVM form (which is required for this output type)."
                );
            }
        }
    }

    List<string> ProduceFinalSources(BuildSettings settings) {
        IEnumerable<IntermediateFile> intermediates = files.Values
            .Select(file => file.Intermediate)
            .Where(intermediate => intermediate != null);

        if (settings.LinkBuiltins) {
            IntermediateFile builtinsIntermediate;

            if (shouldGetIR(settings)) {
                builtinsIntermediate = new IntermediateFile(
                    IntermediateFile.IntermediateType.IR,
                    Utils.JoinPaths(Utils.EPSLLIBS(), "builtins.bc"), false
                );
            } else {
                builtinsIntermediate = new IntermediateFile(
                    IntermediateFile.IntermediateType.Obj,
                    Utils.JoinPaths(Utils.EPSLLIBS(), "builtins.o"), false
                );
            }

            intermediates = intermediates.Concat([builtinsIntermediate]);
        }

        var grouped = intermediates
            .GroupBy(intermediate => intermediate.FileType)
            .ToDictionary();

        if (settings.Output_Type.DoesRequireLLVM()) {
            VerifyOnlyLLVMSources(settings, grouped);
        }

        List<string> sources = [];

        if (settings.OptLevel >= OptimizationLevel.MAX) {
            sources.Add(CombineAndOptimizeLLVM(
                grouped.GetOrEmpty(IntermediateFile.IntermediateType.IR)));
        } else if (settings.OptLevel <= OptimizationLevel.MIN) {
            sources.AddRange(grouped.GetOrEmpty(IntermediateFile.IntermediateType.IR)
                .Select(intermediate => intermediate.Path));
        } else {
            sources.AddRange(OptimizeLLVMIndividually(
                grouped.GetOrEmpty(IntermediateFile.IntermediateType.IR)));
        }

        sources.AddRange(grouped.GetOrEmpty(IntermediateFile.IntermediateType.Obj)
            .Select(intermediate => intermediate.Path));

        return sources;
    }

    void DeleteUnwantedIntermediaries() {
        foreach (FileTree file in files.Values) {
            if (file.GeneratedEPSLSPEC == null) {
                if (file.IRIsInUserDir)
                    Utils.TryDelete(file.IR);
                if (file.ObjIsInUserDir)
                    Utils.TryDelete(file.Obj);
            }
        }
    }

    void ShowCompilationError(SyntaxErrorException e, string text, string file) {
        CodeSpan span = e.span;

        if (span == null || text == null) return;

        int startLine = 1;
        int endLine = 1;
        int totalLines = 1;
        int stage = 0;
        int startIndex = 0;

        List<string> lines = [""];

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
                lines[^1] += chr;
                if (stage == 0) startIndex++;
            }
        }
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("compilation error at ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"{file}:{startLine}");
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

            string line = lines[startLine-1];
            while (line.Length > 0 && Utils.Whitespace.Contains(line[0])) {
                line = line[1..];
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

    void ToExecutable(BuildInfo buildInfo) {
        if (buildInfo.FileWithMain == null) {
            throw new ProjectProblemException("One main function is required when creating an executable; no main function found");
        }

        Log.Status("Buiding executable");
        CmdUtils.ClangToExecutable(buildInfo.Sources, buildInfo.Output);
    }

    void ToLLVM(BuildInfo buildInfo, bool toLL) {
        CmdUtils.LinkLLVM(buildInfo.Sources, buildInfo.Output, toLL);
    }

    void ToPackage(BuildInfo buildInfo, bool includeLLVMInPackage) {
        string outputName = buildInfo.OutputName;
        string output = buildInfo.Output;

        ReadyPackageFolder(output);

        string objFilename = outputName+".o";
        string objPath = Utils.JoinPaths(output, objFilename);

        string irFilename = null;
        if (includeLLVMInPackage) {
            irFilename = outputName+".bc";
            string irPath = Utils.JoinPaths(output, irFilename);

            CmdUtils.LinkLLVM(buildInfo.Sources, irPath);
            CmdUtils.LLVMToObj(irPath, objPath);
        } else {
            CmdUtils.FilesToObject(buildInfo.Sources, objPath);
        }

        IEnumerable<string> unlinkedImports = buildInfo.UnlinkedFiles
            .Select(file => file.PartialPath);

        string sourceFilename = null;

        EPSLSPEC entryEPSLSPEC = buildInfo.EntryFile.EPSLSPEC;
        EPSLSPEC newEPSLSPEC = new(
            entryEPSLSPEC.Functions, entryEPSLSPEC.Structs, Dependencies.Empty(),
            Subconfigs.ToSubconfigCollection(), unlinkedImports,
            irFilename, objFilename, sourceFilename, FileSourceType.Library,
            entryEPSLSPEC.IDPath
        );

        string epslspecDest = Utils.JoinPaths(output, outputName+".epslspec");
        BinJSONEnv.WriteFile(epslspecDest, newEPSLSPEC.ToJSON(this));
    }

    void ToObject(BuildInfo buildInfo) {
        CmdUtils.FilesToObject(buildInfo.Sources, buildInfo.Output);
    }

    void ToSharedObject(BuildInfo buildInfo) {
        CmdUtils.ToSharedObject(buildInfo.Sources, buildInfo.Output);
    }

    public ResultStatus Teardown(EPSLCACHE cache) {
        return RunWrapped(() => {
            Log.Status("Cleaning up EPSLSPECs");
            foreach (string spec in cache.EPSLSPECS) {
                CleanupSPEC(spec);
            }

            Log.Status("Deleting cache file");
            Utils.TryDelete(cache.Path);
        });
    }

    public void WriteEPSLPROJ(string path, IJSONValue json) {
        PrettyPrintConfig printConfig = new(8, 80);
        string epslprojStr = json.PrettyPrint(printConfig);
        File.WriteAllText(Utils.SetExtension(path, "epslproj"), epslprojStr);
    }

    public ResultStatus CreateEPSLPROJ(string path) {
        return RunWrapped(() => {
            string projectName = Utils.GetFileNameWithoutExtension(path);
            string fullPath = Utils.GetFullPath(path);
            Utils.CreateFileUnlessExists(Utils.SetExtension(fullPath, "epsl"));
            string epslprojLocation = Utils.JoinPaths(
                Utils.GetDirectoryName(fullPath), "entry.epslproj");
            if (!Utils.FileExists(epslprojLocation)) {
                JSONObject projJSON = [];
                if (projectName != "entry") {
                    projJSON["file"] = new JSONString(projectName);
                }
                WriteEPSLPROJ(epslprojLocation, projJSON);
            }
        });
    }

    ShapedJSON ReadEPSLPROJ(string path) {
        IJSONValue json = ParseJSONFile(Utils.SetExtension(path, "epslproj"));
        return new ShapedJSON(json, EPSLPROJShape);
    }

    string PackageJSONLocation() {
        return Utils.JoinPaths(Utils.PackagesDir(), "packages.json");
    }

    IEnumerable<Package> GetDefaultPackages() {
        return [
            new Package("consolestyles", Utils.JoinPaths(
                Utils.ProjectAbsolutePath(), "EPSL-Console-Styles"
            ), null, []),
            new Package("json", Utils.JoinPaths(
                Utils.ProjectAbsolutePath(), "EPSL-JSON"
            ), null, ["consolestyles"]),
            new Package("irgen", Utils.JoinPaths(
                Utils.ProjectAbsolutePath(), "EPSL-IR-Gen"
            ), null, []),
            new Package("eewriter", Utils.JoinPaths(
                Utils.ProjectAbsolutePath(), "EEWriter"
            ), null, ["irgen"]),
        ];
    }

    IEnumerable<string> GetEPSLPROJPackageNames(ShapedJSON json) {
        if (!json.HasKey("packages")) return [];
        return json["packages"].IterList().Select(item => item.GetString());
    }

    void WritePackageJSON(IEnumerable<Package> packages) {
        File.WriteAllText(
            PackageJSONLocation(),
            new JSONObject {{"packages", new JSONList(
                packages.Select(package => package.GetJSON())
            )}}.Stringify()
        );
    }

    IEnumerable<Package> ReadPackageJSON() {
        if (!File.Exists(PackageJSONLocation())) {
            IEnumerable<Package> packages = GetDefaultPackages();
            WritePackageJSON(packages);
            return packages;
        }
        ShapedJSON shapedJSON = new(
            ParseJSONFile(PackageJSONLocation()), Package.PackagesShape);
        return shapedJSON["packages"].IterList().Select(Package.FromJSON);
    }

    string GetPackageName(ShapedJSON projJSON) {
        if (!projJSON.HasKey("file")) {
            throw new PackagingProblemException(
                "Can't determine the name of this package as it does not specify a name with the 'file' field"
            );
        }
        return projJSON["file"].GetString();
    }

    IEnumerable<string> GetPackageDependencies(ShapedJSON projJSON) {
        if (!projJSON.HasKey("packages")) {
            return [];
        }
        return projJSON["packages"].IterList()
            .Select(dependency => dependency.GetString());
    }

    public ResultStatus AddPackageToEPSLPROJ(string package, string project) {
        return RunWrapped(() => {
            ShapedJSON json = ReadEPSLPROJ(project);
            IEnumerable<string> packageNames = GetEPSLPROJPackageNames(json);
            if (packageNames.Contains(package)) {
                throw new PackagingProblemException($"{package} is already installed");
            }
            json.GetObject()["packages"] = new JSONList(packageNames
                .Concat([package]).Select(name => new JSONString(name)));
            WriteEPSLPROJ(project, json.GetJSON());
        });
    }

    public ResultStatus RemovePackageFromEPSLPROJ(string package, string project, bool silenceOut=false) {
        return RunWrapped(() => {
            ShapedJSON json = ReadEPSLPROJ(project);
            IEnumerable<string> packageNames = GetEPSLPROJPackageNames(json);
            if (!packageNames.Contains(package)) {
                if (silenceOut) return;
                throw new PackagingProblemException($"{package} isn't installed");
            }
            json.GetObject()["packages"] = new JSONList(packageNames
                .Where(name => name != package)
                .Select(name => new JSONString(name)));
            WriteEPSLPROJ(project, json.GetJSON());
        });
    }

    void DisplayPackageDetails(Package package) {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(package.Name);
        Console.ResetColor();
        Console.WriteLine($"Installed at: {package.Path}");
        Console.Write("Source: ");
        if (package.Source == null) {
            Console.WriteLine("\x1b[3m(none)\x1b[0m");
        } else {
            Console.WriteLine(package.Source);
        }
        Console.Write("Dependencies: ");
        if (package.Dependencies.Any()) {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(string.Join(", ", package.Dependencies));
            Console.ResetColor();
        } else {
            Console.WriteLine("\x1b[3m(none)\x1b[0m");
        }
        Console.WriteLine();
    }

    public ResultStatus ListPackages(string project, bool allPackages) {
        return RunWrapped(() => {
            IEnumerable<Package> packages = ReadPackageJSON();
            if (!allPackages) {
                ShapedJSON json = ReadEPSLPROJ(project);
                IEnumerable<string> projPackages = GetEPSLPROJPackageNames(json);
                packages = packages.Where(package => projPackages.Contains(package.Name));
            }

            foreach (Package package in packages) {
                DisplayPackageDetails(package);
            }
        });
    }

    void RegisterPackage(Package newPackage) {
        IEnumerable<Package> packages = ReadPackageJSON();
        if (packages.Any(package => package.Name == newPackage.Name)) {
            throw new PackagingProblemException(
                "Duplicate package names are not allowed", newPackage
            );
        }
        WritePackageJSON(packages.Concat([newPackage]));
    }

    public ResultStatus RegisterProjectAsPackage(string projectPath) {
        return RunWrapped(() => {
            ShapedJSON epslprojJSON = ReadEPSLPROJ(projectPath);
            RegisterPackage(new Package(
                GetPackageName(epslprojJSON), Utils.GetDirectoryName(projectPath),
                source: null, GetPackageDependencies(epslprojJSON)
            ));
        });
    }

    void DownloadFile(string url, string dest) {
        Log.Status($"Downloading file from {url} to {dest}");
        Uri uri = new(url);
        if (!uri.IsAbsoluteUri) {
            throw new UriFormatException($"The specified URL must be absolute, {url} is not");
        }
        using HttpClient client = new();
        Task<Stream> stream = client.GetStreamAsync(uri);
        using Stream file = new FileStream(dest, FileMode.OpenOrCreate);
        stream.Result.CopyTo(file);
    }

    void ExtractFolder(string src, string dest) {
        try {
            ZipFile.ExtractToDirectory(src, dest);
        } catch (InvalidDataException) {
            throw new PackagingProblemException(
                "Can't install package: don't know how to handle the downloaded file"
            );
        }
    }

    void TryUnwrapRedundantFolder(string dir) {
        string[] subdirectories = Directory.GetDirectories(dir);
        if (Directory.GetFiles(dir).Length == 0
            && subdirectories.Length == 1) {
            string unwrappedTemp = Utils.JoinPaths(Utils.TempDir(), "unwrapped");
            Directory.Move(subdirectories[0], unwrappedTemp);
            Directory.Delete(dir, recursive: true);
            Directory.Move(unwrappedTemp, dir);
        }
    }

    public ResultStatus InstallPackage(string url, string addToProject) {
        string packageName = null;

        ResultStatus status1 = RunWrapped(() => {
            int dotIdx = url.LastIndexOf('.');
            string extension = dotIdx == -1 ? "" : url[(dotIdx + 1)..];
            if (extension.Any(chr => !Utils.NameChars.Contains(chr))) {
                extension = "";
            }
            string folderDest = Utils.JoinPaths(Utils.TempDir(), "package");

            if (extension == "git") {
                CmdUtils.GitClone(url, folderDest);
            } else {
                string downloadDest = Utils.JoinPaths(Utils.TempDir(), "download." + extension);
                DownloadFile(url, downloadDest);
                ExtractFolder(downloadDest, folderDest);
                TryUnwrapRedundantFolder(folderDest);
            }

            string projFileLocation = Utils.JoinPaths(folderDest, "entry.epslproj");
            if (!File.Exists(projFileLocation)) {
                throw new PackagingProblemException(
                    "The downloaded package does not contain a entry.epslproj file"
                );
            }

            ShapedJSON projJSON = ReadEPSLPROJ(projFileLocation);
            packageName = GetPackageName(projJSON);

            if (ReadPackageJSON().Any(package => package.Name == packageName)) {
                throw new PackagingProblemException(
                    $"A package named '{packageName}' has already been installed"
                );
            }

            string installDest = Utils.JoinPaths(Utils.PackagesDir(), packageName);
            if (Directory.Exists(installDest)) {
                Directory.Delete(installDest, recursive: true);
            }
            Directory.Move(folderDest, installDest);

            CmdUtils.RunScript("epslc.py", ["compile", installDest + Path.DirectorySeparatorChar]);

            IEnumerable<string> dependencies = GetPackageDependencies(projJSON);
            RegisterPackage(new Package(
                packageName, installDest, url, dependencies
            ));
        });

        if (status1 != ResultStatus.GOOD) return status1;

        try {
            ReadEPSLPROJ(addToProject);
            return AddPackageToEPSLPROJ(packageName, addToProject);
        } catch (IOException) { }

        return ResultStatus.GOOD;
    }

    public ResultStatus UninstallPackage(string packageName, string project) {
        try {
            ReadEPSLPROJ(project);
            RemovePackageFromEPSLPROJ(packageName, project, silenceOut: true);
        } catch (IOException) {};
        return RunWrapped(() => {
            List<Package> packages = ReadPackageJSON().ToList();
            Package uninstallee = packages
                .FirstOrDefault(package => package.Name == packageName);
            if (uninstallee == null) {
                throw new PackagingProblemException(
                    $"No package by the name '{packageName}' can be found"
                );
            }
            if (Utils.IsPathIn(uninstallee.Path, Utils.PackagesDir())) {
                Log.Status("Deleting directory", uninstallee.Path);
                Directory.Delete(uninstallee.Path, recursive: true);
            } else {
                Console.WriteLine("The package's files have not been removed as the package was installed manually");
            }
            packages.Remove(uninstallee);
            WritePackageJSON(packages);
        });
    }

    IJSONValue ParseJSONFile(string path) {
        currentFile = path;
        return JSONTools.ParseJSONFile(path, text => currentText = text);
    }
}

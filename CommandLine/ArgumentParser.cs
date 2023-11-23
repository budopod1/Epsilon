using System;
using System.Linq;
using System.Collections.Generic;

public class ArgumentParser {
    ParserTree tree = new ParserTree("", null);
    ParserTree currentTree;
    Dictionary<string, string> options = new Dictionary<string, string> {
        {"-h", "Show usage information"},
        {"--help", "Show usage information"},
    };

    public ArgumentParser() {
        currentTree = tree;
    }

    public void AddOption(string option, string help) {
        options[option] = help;
    }

    public void AddBranch(string name) {
        ParserTree branch = new ParserTree(name, currentTree);
        currentTree.AddNode(branch);
        currentTree = branch;
    }

    public void AddLeaf(string name) {
        ParserLeaf branch = new ParserLeaf(name, currentTree);
        currentTree.AddNode(branch);
    }

    public void Up() {
        currentTree = currentTree.GetParent();
    }

    public void DisplayProblem(string problem) {
        Console.WriteLine(problem);
        Console.WriteLine("Use '-h' to view usage");
        Environment.Exit(0);
    }

    void TestOption(string option) {
        if (!options.ContainsKey(option)) {
            DisplayProblem($"Invalid option '{option}'");
        }
    }

    void ShowUsage(IParserNode node, string indent) {
        string content = node.GetContent();
        if (content.Length > 0) {
            if (content[0] == '*') {
                Console.WriteLine(indent+$"<{content.Substring(1)}>");
            } else {
                Console.WriteLine(indent+content);
            }
        }
        ParserTree ctree = node as ParserTree;
        if (ctree != null) {
            List<IParserNode> nodes = ctree.GetNodes();
            foreach (IParserNode sub in nodes)
                ShowUsage(sub, indent+Utils.Tab);
        }
    }

    public void ShowHelp() {
        Console.WriteLine("Usage:");
        ShowUsage(tree, "");
        Console.WriteLine();
        Console.WriteLine("Options:");
        foreach (KeyValuePair<string, string> pair in options) {
            Console.WriteLine(pair.Key + ": " + pair.Value);
        }
        Environment.Exit(0);
    }
    
    public ParserResults Parse(string[] args) {
        if (args.Length == 0) ShowHelp();
        List<string> usedOptions = new List<string>();
        List<string> mode = new List<string>();
        List<string> values = new List<string>();
        ParserTree ctree = tree;
        bool finished = false;
        for (int i = 0; i < args.Length; i++) {
            string arg = args[i];

            if (arg == "-h" || arg == "--help")
                ShowHelp();
            
            if (arg.StartsWith("-") && arg.Length >= 2) {
                string sliced = arg.Substring(1);
                if (sliced[0] == '-') {
                    string option = sliced.Substring(1);
                    TestOption(option);
                    usedOptions.Add(option);
                    continue;
                } else {
                    foreach (char chr in sliced) {
                        string option = chr.ToString();
                        TestOption(option);
                        usedOptions.Add(option);
                    }
                    continue;
                }
            }
            
            bool foundMatch = false;
            foreach (IParserNode node in ctree.GetNodes()) {
                bool isValuePlaceholder = node.GetContent()[0] == '*';
                if (isValuePlaceholder || node.GetContent() == arg) {
                    foundMatch = true;
                    if (isValuePlaceholder) {
                        values.Add(arg);
                    } else {
                        mode.Add(arg);
                    }
                    ParserLeaf leaf = node as ParserLeaf;
                    if (leaf != null) {
                        finished = true;
                    }
                    ParserTree tree = node as ParserTree;
                    if (tree != null) {
                        ctree = tree;
                    }
                    break;
                }
            }
            
            if (finished) {
                if (i == args.Length-1) {
                    break;
                } else {
                    DisplayProblem("To many arguments");
                }
            }
            
            if (!foundMatch) {
                DisplayProblem($"Invalid argument {arg}, expected {Utils.ENList(ctree.GetNodes().Select(node=>node.GetContent()).ToList(), "or")}");
            }
        }
        if (!finished)
            DisplayProblem("Incomplete command");
        return new ParserResults(usedOptions, mode, values);
    }
}

using System;
using System.Linq;
using System.Collections.Generic;

public class ArgumentParser {
    string cmdName;
    string description;
    SwitchableDeque<Expectation> expected = new SwitchableDeque<Expectation>(false);
    Dictionary<string, Expectation> options = new Dictionary<string, Expectation>();
    List<(string, string, string[])> optionHelp = new List<(string, string, string[])>();

    public int OptionUsagePadding = 45;

    public ArgumentParser(string cmdName, string description) {
        this.cmdName = cmdName;
        this.description = description;
        AddOption(ShowHelp, "Show this text and exit", "h", "help");
    }

    public void DisplayProblem(string problem) {
        Console.WriteLine("Epsilon: "+problem);
        Console.WriteLine("Use '--help' to view usage");
        Environment.Exit(1);
    }
    
    public void ShowHelp() {
        Console.WriteLine(description);
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.Write(cmdName + " [options] ");
        foreach (Expectation expectation in expected) {
            if (expectation.IsEmpty()) continue;
            string help = expectation.GetHelp();
            if (expectation.IsOptional()) help = $"[{help}]";
            Console.Write(help + " ");
        }
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Options:");
        foreach ((string help, string expected, string[] names) in optionHelp) {
            bool first = true;
            string optionUsage = "";
            foreach (string name in names) {
                if (!first) optionUsage += ", ";
                first = false;
                if (name.Length == 1) {
                    optionUsage += "-"+name;
                } else {
                    optionUsage += "--"+name;
                }
            }
            if (expected != "") optionUsage += " "+expected;
            Console.Write(optionUsage.PadRight(OptionUsagePadding));
            Console.Write(" " + help);
            Console.WriteLine();
        }
        Environment.Exit(0);
    }

    public T Expect<T>(T expectation) where T : Expectation {
        expected.Add(expectation);
        return expectation;
    }

    public T AddOption<T>(T expectation, string help, params string[] names) where T : Expectation {
        optionHelp.Add((help, expectation.GetHelp(), names));
        foreach (string name in names) {
            options[name] = expectation;
        }
        return expectation;
    }

    public ActionExpectation AddOption(Action action, string help, params string[] names) {
        optionHelp.Add((help, "", names));
        ActionExpectation expectation = new ActionExpectation(action);
        foreach (string name in names) {
            options[name] = expectation;
        }
        return expectation;
    }

    public void UseOption(string option) {
        if (!options.ContainsKey(option)) {
            DisplayProblem($"Unknown option {JSONTools.ToLiteral(option)}");
        }
        Expect(options[option]);
    }

    public void Parse(string[] args) {
        expected.ToStack();
        bool positionalOnly = false;
        foreach (string arg in args) {
            if (arg == "--") {
                positionalOnly = true;
                continue;
            }
            
            if (!positionalOnly && arg.Length >= 2 && arg[0] == '-') {
                if (arg[1] == '-') {
                    string option = arg.Substring(2);
                    UseOption(option);
                } else {
                    string sliced = arg.Substring(1);
                    foreach (char chr in sliced) {
                        string option = chr.ToString();
                        UseOption(option);
                    }
                }
                continue;
            }

            while (true) {
                if (expected.Count == 0) {
                    DisplayProblem($"To many parameters: {JSONTools.ToLiteral(arg)}");
                }
                Expectation expectation = expected.Pop();
                expectation.IsPresent = true;
                if (expectation.IsEmpty()) {
                    expectation.RunThens();
                    continue;
                }
                bool matches = expectation.Matches(arg);
                if (!matches) {
                    if (!expectation.IsOptional()) {
                        DisplayProblem("Expected " + expectation.GetHelp());
                    }
                    continue;
                }
                expectation.Matched = arg;
                expectation.RunThens();
                break;
            }
        }

        while (expected.Count > 0) {
            Expectation expectation = expected.Pop();
            if (expectation.IsEmpty() || expectation.IsOptional()) {
                expectation.IsPresent = true;
                expectation.RunThens();
                continue;
            }
            DisplayProblem("Expected " + expectation.GetHelp());
        }
    }
}

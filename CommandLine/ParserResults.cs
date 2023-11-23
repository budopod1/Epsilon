using System;
using System.Collections.Generic;

public class ParserResults {
    List<string> options;
    List<string> mode;
    List<string> values;
    
    public ParserResults(List<string> options, List<string> mode, List<string> values) {
        this.options = options;
        this.mode = mode;
        this.values = values;
    }

    public bool HasOption(string option) {
        return options.Contains(option);
    }

    public List<string> GetOptions() {
        return options;
    }

    public List<string> GetMode() {
        return mode;
    }

    public List<string> GetValues() {
        return values;
    }
}

using System;
using System.Collections.Generic;

public class SPECParser {
    ISPECVal ParseVal(string val, int lineNum) {
        string content = val.Substring(1);
        switch (val[0]) {
            case '{':
                return new SPECObj();
            case '\"':
                return new SPECStr(content);
            case '[':
                return new SPECList();
            default:
                throw new SPECSyntaxErrorException($"Illegal value on line {lineNum}");
        }
    }
    
    public SPECObj Parse(string text) {
        SPECObj result = new SPECObj();
        Stack<ISPECVal> stack = new Stack<ISPECVal>();
        stack.Push(result);
        int lastIndent = 0;
        int lineNum = 0;
        foreach (string wholeLine in text.Split("\n")) {
            lineNum++;
            int indentAmount = 0;
            foreach (char chr in wholeLine) {
                if (chr == ' ') {
                    indentAmount++;
                } else {
                    break;
                }
            }
            string line = wholeLine.Substring(indentAmount);
            int indent = indentAmount / 4;
            if (indent != lastIndent && indent != lastIndent-1) {
                throw new SPECSyntaxErrorException($"Illegal indent on line {lineNum}");
            }
            if (indent == lastIndent-1) {
                stack.Pop();
            }
            if (line != "" && line != "]" && line != "}" && !(line[0] == '#')) {
                ISPECVal ctx = stack.Peek();
                ISPECVal val = null;
                if (ctx is SPECList) {
                    val = ParseVal(line, lineNum);
                    ((SPECList)ctx).Add(val);
                } else if (ctx is SPECObj) {
                    int colonIdx = line.IndexOf(':');
                    if (colonIdx == -1) {
                        throw new SPECSyntaxErrorException(
                            $"Value on line {lineNum} in object must have colon"
                        );
                    }
                    string key = line.Substring(0, colonIdx);
                    string value = line.Substring(colonIdx+1).TrimStart();
                    val = ParseVal(value, lineNum);
                    ((SPECObj)ctx)[key] = val;
                }
                if (val is SPECList || val is SPECObj) {
                    stack.Push(val);
                    indent++;
                }
            }
            lastIndent = indent;
        }
        return result;
    }
}

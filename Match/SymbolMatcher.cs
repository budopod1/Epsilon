using System;
using System.Reflection;
using System.Collections.Generic;

public class SymbolMatcher : IMatcher {
    Dictionary<string, Type> symbols;

    public SymbolMatcher(Dictionary<string, Type> symbols) {
        this.symbols = symbols;
    }
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            List<string> possibleSymbols = new List<string>(this.symbols.Keys);
            List<Token> replaced = new List<Token>();
            int k = -1;
            for (int j = i; j < tokens.Count; j++) {
                k++;
                Token token = tokens[j];
                if (!(token is TextToken)) break;
                replaced.Add(token);
                char chr = ((TextToken)token).GetText()[0];
                for (int l = 0; l < possibleSymbols.Count; l++) {
                    string symbol = possibleSymbols[l];
                    if (symbol[k] != chr) {
                        possibleSymbols.RemoveAt(l);
                        l--; // account for a removal shifting later
                        // items down by one
                        continue;
                    }
                    if (k == symbol.Length-1) {
                        List<Token> replacement = new List<Token>();
                        Type type = this.symbols[symbol];
                        if (type != null) {
                            Object result = Activator.CreateInstance(this.symbols[symbol]);
                            replacement.Add((Token)result);
                        }
                        return new Match(i, j, replacement, replaced);
                    }
                }
                if (possibleSymbols.Count == 0) {
                    break;
                }
            }
        }
        return null;
    }
}

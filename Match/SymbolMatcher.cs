using System;
using System.Reflection;
using System.Collections.Generic;

public class SymbolMatcher : IMatcher {
    Dictionary<string, Type> symbols;

    public SymbolMatcher(Dictionary<string, Type> symbols) {
        this.symbols = symbols;
    }
    
    public Match Match(IToken tokens_) {
        if (!(tokens_ is TreeToken)) return null;
        TreeToken tokens = (TreeToken)tokens_;
        for (int i = 0; i < tokens.Count; i++) {
            List<string> possibleSymbols = new List<string>(this.symbols.Keys);
            List<IToken> replaced = new List<IToken>();
            int k = -1;
            for (int j = i; j < tokens.Count; j++) {
                k++;
                IToken token = tokens[j];
                if (!(token is TextToken)) break;
                replaced.Add(token);
                char chr = ((TextToken)token).Text[0];
                for (int l = 0; l < possibleSymbols.Count; l++) {
                    string symbol = possibleSymbols[l];
                    if (symbol[k] != chr) {
                        possibleSymbols.RemoveAt(l);
                        l--; // account for a removal shifting later
                        // items down by one
                        continue;
                    }
                    if (k == symbol.Length-1) {
                        List<IToken> replacement = new List<IToken>();
                        Type type = this.symbols[symbol];
                        if (type != null) {
                            Object result = Activator.CreateInstance(this.symbols[symbol]);
                            replacement.Add((IToken)result);
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

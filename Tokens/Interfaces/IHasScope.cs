using System;

public interface IHasScope : IToken {
    IScope GetScope();
}

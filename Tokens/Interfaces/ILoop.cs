using System;

public interface ILoop : IFlowControl {
    CodeBlock GetBlock();
}

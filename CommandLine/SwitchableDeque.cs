using System;
using System.Collections.Generic;

public class SwitchableDeque<T> : LinkedList<T> {
    bool stackMode = false;

    public SwitchableDeque(bool isStack) {
        stackMode = isStack;
    }

    public void ToStack() {
        stackMode = true;
    }

    public void ToQueue() {
        stackMode = false;
    }

    public void Add(T elem) {
        if (stackMode) {
            AddFirst(elem);
        } else {
            AddLast(elem);
        }
    }
    
    public T Pop() {
        T result = First.Value;
        RemoveFirst();
        return result;
    }
}

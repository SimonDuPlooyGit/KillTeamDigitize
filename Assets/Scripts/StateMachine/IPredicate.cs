using System;
using UnityEngine;

public interface IPredicate
{
    bool Evaluate();
}

public class FuncPredicate : IPredicate
{
    readonly Func<bool> func;

    public FuncPredicate(Func<bool> func)
    {
        this.func = func;
    }

    public bool Evaluate() => func.Invoke();
}

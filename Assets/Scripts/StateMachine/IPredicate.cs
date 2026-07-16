using System;
using UnityEngine;

public interface IPredicate
{
    //The interface for a predicate function
    bool Evaluate();
    //^ Every class implementing IPredicate is guaranteed to have an Evaluate function via "contract"
    //The classes can decide what logic their Evaluate() will have
}

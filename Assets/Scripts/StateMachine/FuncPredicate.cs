using System;
//-----FuncPredicate Class (Class that inherits IPredicate. It evaluates a predicate, usually for checking state transition triggers)-----
public class FuncPredicate : IPredicate
{
    //Implements the IPredicate interface
    
    readonly Func<bool> _func; //Has a delegate function variable that returns a boolean (hence a predicate)

    public FuncPredicate(Func<bool> func) //Constructor to assign this func with a provided predicate
    {
        _func = func;
    }

    public bool Evaluate() => _func.Invoke();
    //The evaluate function will invoke the predicate assigned.
    //The listener for this invoke is subscribed in GameManager awake with the State constructors
}
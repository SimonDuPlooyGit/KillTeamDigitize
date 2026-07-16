using System;
public class FuncPredicate : IPredicate
{
    //Implements the IPredicate interface
    
    readonly Func<bool> func; //Has a delegate function variable that returns a boolean (hence a predicate)

    public FuncPredicate(Func<bool> func) //Constructor to assign this func with a provided predicate
    {
        this.func = func;
    }

    public bool Evaluate() => func.Invoke();
    //The evaluate function will invoke the predicate assigned.
    //The listener for this invoke is subscribed in GameManager awake with the State constructors
}

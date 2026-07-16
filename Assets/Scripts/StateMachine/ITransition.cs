using UnityEngine;

public interface ITransition
{
    //Interface for transitions
    //Guarantees a class implementing ITransitions has these variables and methods
    
    IState To { get; } //Get-only auto property. Classes implementing ITransition have to provide a public way to read this IState variable
    IPredicate Condition { get; } //Get-only auto property. Classes implementing ITransition have to provide a public way to read this IPredicate variable
}

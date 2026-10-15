//-----IPredicate Interface (Says child classes must have an Evaluate method that returns a bool)-----
public interface IPredicate
{
    //The interface for a predicate function
    bool Evaluate();
    //^ Every class implementing IPredicate is guaranteed to have an Evaluate function via "contract"
    //The classes can decide what logic their Evaluate() will have
}

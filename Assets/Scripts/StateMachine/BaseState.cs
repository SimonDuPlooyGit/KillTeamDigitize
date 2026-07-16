using UnityEngine;

public abstract class BaseState : IState
{
    //A default base state to be in, implementing IState
    
    protected readonly InformationPackage Context; //Needs to be able to read the Context information package where information is sent around

    protected BaseState(InformationPackage context) //Constructor to make a base state assigning the Context to the parameter fed context
    {
        Context = context;
    }

    //Abstract methods can be overriden by classes (states) that inherit from BaseState
    public abstract void OnEnter(); //Abstract method for when a state is entered
    public abstract void Update(); //Abstract method for when a state is updating
    public abstract void OnExit(); //Abstract method for when a state is exited
}

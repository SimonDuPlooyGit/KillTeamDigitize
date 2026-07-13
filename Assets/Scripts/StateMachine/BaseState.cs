using UnityEngine;

public abstract class BaseState : IState
{
    protected readonly InformationPackage Context;

    protected BaseState(InformationPackage context)
    {
        Context = context;
    }

    protected BaseState()
    {
        //no operation
    }

    public abstract void OnEnter();
    public abstract void Update();
    public abstract void OnExit();
}

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
        throw new System.NotImplementedException();
    }

    public abstract void OnEnter();
    public abstract void Update();
    public abstract void OnExit();
}

using UnityEngine;

public class CombatState : BaseState
{
    private readonly InputActions _input;

    public CombatState(InformationPackage context, InputActions input)
    {
        _input = input;
    }
    
    public override void OnEnter()
    {
        Debug.Log("CombatState entered");
    }

    public override void Update()
    {
        //No operation
    }

    public override void OnExit()
    {
        Debug.Log("CombatState Exited");
    }
}

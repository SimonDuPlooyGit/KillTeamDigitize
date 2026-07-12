using UnityEngine;

public class WeaponSelectState : BaseState
{
    public WeaponSelectState(InformationPackage context) : base(context) { }
    
    public override void OnEnter()
    {
        Debug.Log("WeaponSelectState entered");
    }

    public override void Update()
    {
        throw new System.NotImplementedException();
    }

    public override void OnExit()
    {
        Debug.Log("WeaponSelectState exited");
    }
}

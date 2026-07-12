using UnityEngine;

public class ActionSelectionState : BaseState
{
    public ActionSelectionState(InformationPackage context) : base(context) { }


    public override void OnEnter()
    {
        Debug.Log("ActionSelectionState entered");
    }

    public override void Update()
    {
        throw new System.NotImplementedException();
    }

    public override void OnExit()
    {
        Debug.Log("ActionSelectionState exited");
    }
}

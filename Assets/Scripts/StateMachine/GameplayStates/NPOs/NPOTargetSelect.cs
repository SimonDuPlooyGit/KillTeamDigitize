using UnityEngine;

public class NPOTargetSelect : BaseState
{
    public NPOTargetSelect(InformationPackage context) : base(context)
    {
    }

    public override void OnEnter()
    {
        Debug.Log("NPOTargetSelect OnEnter");
    }

    public override void Update()
    {
        //no op
    }

    public override void OnExit()
    {
        Debug.Log("NPOTargetSelect OnExit");
    }
}

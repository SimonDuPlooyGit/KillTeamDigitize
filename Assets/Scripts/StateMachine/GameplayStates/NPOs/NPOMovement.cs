using UnityEngine;

public class NPOMovement : BaseState
{
    public NPOMovement(InformationPackage context) : base(context)
    {
        
    }

    public override void OnEnter()
    {
        Debug.Log("NPOMovement OnEnter");
    }

    public override void Update()
    {
        //no op
    }

    public override void OnExit()
    {
        Debug.Log("NPOMovement OnExit");
    }
}

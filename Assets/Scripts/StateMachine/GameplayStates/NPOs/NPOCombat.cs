using UnityEngine;

public class NPOCombat : BaseState
{
    public NPOCombat(InformationPackage context) : base(context)
    {
    }

    public override void OnEnter()
    {
        Debug.Log("NPOCombat OnEnter");
    }

    public override void Update()
    {
        //no op
    }

    public override void OnExit()
    {
        Debug.Log("NPOCombat OnExit");
    }
}

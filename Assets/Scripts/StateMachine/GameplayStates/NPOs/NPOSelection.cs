using UnityEngine;

public class NPOSelection : BaseState
{
    private NPOManager _npoManager;
    public NPOSelection(InformationPackage context, NPOManager npoManager) : base(context)
    {
        _npoManager = npoManager;
    }

    public override void OnEnter()
    {
        Debug.Log("NPOSelection OnEnter");
        _npoManager.GetNPOUnitToActivate();
    }

    public override void Update()
    {
        //no op
    }

    public override void OnExit()
    {
        Debug.Log("NPOSelection OnExit");
    }
}

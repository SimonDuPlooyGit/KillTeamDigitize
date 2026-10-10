using UnityEngine;

public class NPOActionSelect : BaseState
{
    private NPOManager _npoManager;
    
    public NPOActionSelect(InformationPackage context, NPOManager npoManager) : base(context)
    {
        _npoManager = npoManager;
    }

    public override void OnEnter()
    {
        Debug.Log("NPOActionSelect OnEnter");
    }

    public override void Update()
    {
        //no op
    }

    public override void OnExit()
    {
        Debug.Log("NPOActionSelect OnExit");
    }
}

using UnityEngine;

//-----NPOSelection Class (Selects an NPO to activate)-----
public class NPOSelection : BaseState
{
    private readonly NPOManager _npoManager;
    public NPOSelection(InformationPackage context, NPOManager npoManager) : base(context)
    {
        _npoManager = npoManager;
    }

    public override void OnEnter()
    {
        Debug.Log("NPOSelection OnEnter");
        Context.CurrentlyActivatedNpo = _npoManager.GetNpoUnitToActivate();
        
        if (Context.CurrentlyActivatedNpo != null)
        {
            Context.CurrentlyActivatedNpo.ResetActionFlags();
        }
        else
        {
            Debug.Log("No remaining NPO units to activate");
        }
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

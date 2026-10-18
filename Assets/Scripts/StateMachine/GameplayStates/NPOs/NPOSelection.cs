using UnityEngine;

//-----NPOSelection Class (Selects an NPO to activate)-----
public class NPOSelection : BaseState
{
    private readonly NPOManager _npoManager;
    private GameManager _gameManager;
    public NPOSelection(InformationPackage context, NPOManager npoManager, GameManager gm) : base(context)
    {
        _npoManager = npoManager;
        _gameManager = gm;
    }

    public override void OnEnter()
    {
        Debug.Log("NPOSelection OnEnter");
        Context.CurrentlyActivatedNpo = _npoManager.GetNpoUnitToActivate();
        _gameManager.mainCamera.SetOrbitTarget(Context.CurrentlyActivatedNpo.transform);
        if (Context.CurrentlyActivatedNpo != null)
        {
            Debug.Log($"Selected NPO: {Context.CurrentlyActivatedNpo.name} (Instance ID: {Context.CurrentlyActivatedNpo.gameObject.GetInstanceID()}), operativeData is: {(Context.CurrentlyActivatedNpo.operativeData == null ? "NULL" : Context.CurrentlyActivatedNpo.operativeData.name)}");
        }
        

        if (Context.CurrentlyActivatedNpo == null)
        {
            Debug.Log("GetNpoUnitToActivate returned null");
            return;
        }
        
        if (Context.CurrentlyActivatedNpo.operativeData == null)
        {
            Debug.LogError($"Found NPO {Context.CurrentlyActivatedNpo.name}, but its operativeData slot is actually null at runtime!");
            return;
        }
        
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

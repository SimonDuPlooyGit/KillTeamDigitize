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
        NonPlayerOperative npoToActivate = _npoManager.GetNpoUnitToActivate();
        Context.SetSelectedOperativesData(npoToActivate);
        _gameManager.mainCamera.SetOrbitTarget(Context.CurrentlySelectedOperative.transform);
        npoToActivate.ResetActionFlags();
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

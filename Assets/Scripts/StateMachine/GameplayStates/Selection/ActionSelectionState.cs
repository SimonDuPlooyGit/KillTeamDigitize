using UnityEngine;

//-----ActionSelectionState (Where player can select an action from action menu)-----
public class ActionSelectionState : BaseState
{
    //Inherits from BaseState
    //State that handles players selecting actions from the action menu (need to check APL costs and other action eligibility)
    
    private readonly MenuPanel _menu; //Will need to have access to the menu
    private GameObject repositionButton;
    private GameObject chargeButton;
    private GameObject dashButton;
    private GameObject repositionCost;
    private GameObject shootButton;
    private GameObject shootCost;
    private GameManager _gameManager;

    public ActionSelectionState(InformationPackage context, MenuPanel menu, GameManager gm) : base(context)
    {
        _menu = menu;
        _gameManager = gm;
    }


    public override void OnEnter()
    {
        _gameManager.mainCamera.SetOrbitTarget(Context.CurrentlySelectedOperative.transform);
        Debug.Log("ActionSelectionState entered");
        _menu.OpenMenu(_menu.actionMenu);
        _menu.OpenMenu(_menu.tpCounter);
        _menu.OpenMenu(_menu.objective);
        Context.IsPlayerActivationRequested = true;
        
        ActivateOrDeactivateActionButtons();
        
        //Get action buttons
        repositionButton = _menu.actionMenu.transform.Find("Buttons").Find("RepositionButton").gameObject;
        chargeButton = _menu.actionMenu.transform.Find("Buttons").Find("ChargeButton").gameObject;
        dashButton = _menu.actionMenu.transform.Find("Buttons").Find("DashButton").gameObject;
        shootButton = _menu.actionMenu.transform.Find("Buttons").Find("ShootButton").gameObject;
        repositionCost = _menu.actionMenu.transform.Find("APLCosts").Find("MoveAPL").gameObject;
        shootCost = _menu.actionMenu.transform.Find("APLCosts").Find("ShootAPL").gameObject;

        //tutorial panel
        if(_menu.tutFlag2)
        {
            _menu.OpenMenu(_menu.tutAction);
        }
        if(_menu.tutFlag4 && _menu.tutFlag4B)
        {
            _menu.OpenMenu(_menu.tutRepoPrompt);
        }
    }

    public override void Update()
    {
        //no operation
    }

    public override void OnExit()
    {
        Debug.Log("ActionSelectionState exited");
        _menu.CloseMenu(_menu.actionMenu);
        _menu.CloseMenu(_menu.tutAction);
        _menu.CloseMenu(_menu.tpCounter);
        _menu.CloseMenu(_menu.objective);
        _menu.CloseMenu(_menu.tutRepoPrompt);
        Context.IsPlayerActivationRequested = false;
    }

    private void ActivateOrDeactivateActionButtons()
    {
        //Activate different action buttons according to what the unit hasn't done yet
        var playerOp = Context.CurrentlySelectedOperativeScript as PlayerOperative;
        if (playerOp == null) return;

        if (repositionButton != null)
        {
            repositionButton.SetActive(!playerOp.hasMovedThisActivation && playerOp.currentApl > 0);
            repositionCost.SetActive(!playerOp.hasMovedThisActivation && playerOp.currentApl > 0);
        }

        if (shootButton != null)
        {
            shootButton.SetActive(!playerOp.hasShotThisActivation && playerOp.currentApl > 0);
            shootCost.SetActive(!playerOp.hasShotThisActivation && playerOp.currentApl > 0);
        }
    }
}

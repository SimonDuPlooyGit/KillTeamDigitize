using UnityEngine;

public class ActionSelectionState : BaseState
{
    //Inherits from BaseState
    //State that handles players selecting actions from the action menu (need to check APL costs and other action eligibility)
    
    private readonly MenuPanel _menu; //Will need to have access to the menu

    public ActionSelectionState(InformationPackage context, MenuPanel menu) : base(context)
    {
        _menu = menu;
    }


    public override void OnEnter()
    {
        Debug.Log("ActionSelectionState entered");
        _menu.OpenMenu(_menu.actionMenu);
    }

    public override void Update()
    {
        //no operation
    }

    public override void OnExit()
    {
        Debug.Log("ActionSelectionState exited");
        _menu.CloseMenu(_menu.actionMenu);
    }
    
    //Future action eligibility checks and logic
}

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
        _menu.OpenMenu(_menu.tpCounter);
        _menu.OpenMenu(_menu.objective);

        //tutorial panel
        if(_menu.tutFlag2)
        {
            _menu.OpenMenu(_menu.tutAction);
        }
        if(_menu.tutFlag4)
        {
            _menu.OpenMenu(_menu.tutShootPrompt);
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
        _menu.CloseMenu(_menu.tutShootPrompt);
    }
    
    //Future action eligibility checks and logic
}

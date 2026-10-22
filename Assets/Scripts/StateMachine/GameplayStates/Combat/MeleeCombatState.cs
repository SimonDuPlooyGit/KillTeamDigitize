using UnityEngine;

public class MeleeCombatState : BaseState
{
    private MenuPanel _menu;
    private DiceHandler _diceHandler;
    
    public MeleeCombatState(InformationPackage context, MenuPanel menu, DiceHandler diceHandler) : base(context)
    {
        _menu = menu;
        _diceHandler = diceHandler;
    }

    public override void OnEnter()
    {
        Debug.Log("MeleeCombatState OnEnter");
        
        Context.DiceHandler = _diceHandler;
        
        //Reset
        Context.AttackRolls.Clear();
        Context.DefenseRolls.Clear();
        Context.RetainedCrits = 0;
        Context.RetainedNormals = 0;
    }

    public override void Update()
    {
        //no op
    }

    public override void OnExit()
    {
        Debug.Log("MeleeCombatState OnExit");
    }
}

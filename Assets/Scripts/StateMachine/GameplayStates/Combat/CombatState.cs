using System.Collections.Generic;
using UnityEngine;

public class CombatState : BaseState
{
    private readonly InputActions _input;
    public CombatManager _combatManager;

    public CombatState(InformationPackage context, InputActions input, CombatManager combatManager) : base(context)
    {
        _input = input;
        _combatManager = combatManager;
    }
    
    public override void OnEnter()
    {
        Debug.Log("CombatState entered");
        EvaluateShootRolls();
    }

    public override void Update()
    {
        //No operation
    }

    public override void OnExit()
    {
        Debug.Log("CombatState Exited");
    }

    public void EvaluateShootRolls()
    {
        _combatManager.targetDefense = Context.targetUnitSO.SAVE;
        _combatManager.SpawnDice(Context.weapon.ATK);
    }
}

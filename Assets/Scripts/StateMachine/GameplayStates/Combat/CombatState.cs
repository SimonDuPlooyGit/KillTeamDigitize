using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatState : BaseState
{
    //Inherits from BaseState.
    //The combat state that handles rolling and rules
    
    public DiceHandler _diceHandler; //Needs reference to the CombatManager script on the CombatManager GameObject

    public CombatState(InformationPackage context, MenuPanel menu, DiceHandler diceHandler) : base(context) //CombatState constructor ": base(context)" is handing context up to the BaseState constructor
    {
        _diceHandler = diceHandler;
    }
    
    public override void OnEnter()
    {
        Debug.Log("CombatState entered");
        
        Context.diceHandler = _diceHandler;
        
        //Reset
        Context.attackRolls.Clear();
        Context.defenseRolls.Clear();
        Context.retainedCrits = 0;
        Context.retainedNormals = 0;
        Context.retainedCritDefense = 0;
        Context.retainedNormalDefense = 0;
        
        _diceHandler.StartCoroutine(ResolveCombat());
    }

    private IEnumerator ResolveCombat()
    {
        Debug.Log("ResolveCombat");
        
        //Pre-Roll
        ExecuteRulesInThisStep(AttackTimings.PreRoll);
        
        //Roll Attack Dice and defense dice
        yield return _diceHandler.StartCoroutine(_diceHandler.ThrowAttackDice(Context.weapon.ATK, true, Context));
        
        //After attack roll
        //Keep track of values before rerolls
        List<int> oldAttackRolls = new List<int>(Context.attackRolls);
        ExecuteRulesInThisStep(AttackTimings.AfterAttackRoll);
        
        //Rerolls
        
        yield return new WaitForSeconds(2f);

        yield return _diceHandler.StartCoroutine(_diceHandler.ThrowDefenseDice(Context.numDefenseDiceRoll, true, Context));
        
        List<int> oldDefenseRolls = new List<int>(Context.defenseRolls);
        
        //Attack Evaluation
        ExecuteRulesInThisStep(AttackTimings.AttackEvaluation);
        EvaluateHitsAndSaves();
        
        //After Attack Evaluation
        ExecuteRulesInThisStep(AttackTimings.AfterAttackEvaluation);
        ApplyFinalDamage();
        
        yield return new WaitForSeconds(1.5f);
        _diceHandler.ClearDiceRolls();
        Context.currentlySelectedUnitScript.lineRenderer.enabled = false;
        Context.isShootingConfirmed = true;
    }
    
    private void EvaluateHitsAndSaves()
    {
        Debug.Log("Evaluating hits and saves");
        for (int i = 0; i < Context.attackRolls.Count; i++)
        {
            if (Context.attackRolls[i] >= Context.weapon.HIT)
            {
                if (Context.attackRolls[i] == 6)
                {
                    Context.retainedCrits += 1;
                    Debug.Log("Number of crits: " + Context.retainedCrits.ToString());
                }
                else
                {
                    Context.retainedNormals += 1;
                    Debug.Log("Number of normals: " + Context.retainedNormals.ToString());
                }
            }
        }

        for (int i = 0; i < Context.defenseRolls.Count; i++)
        {
            if (Context.defenseRolls[i] >= Context.targetUnitSO.SAVE)
            {
                if (Context.defenseRolls[i] == 6)
                {
                    Context.retainedCritDefense += 1;
                    Debug.Log("Number of crit saves: " + Context.retainedCritDefense.ToString());
                }
                else
                {
                    Context.retainedNormalDefense += 1;
                    Debug.Log("Number of normals saves: " + Context.retainedNormalDefense.ToString());
                }
            }
        }

        if (Context.retainedCritDefense >= Context.retainedCrits)
        {
            Context.retainedCritDefense -= Context.retainedCrits;
            Context.retainedCrits = 0;
        } else if (Context.retainedCritDefense < Context.retainedCrits)
        {
            Context.retainedCrits -= Context.retainedCritDefense;
            Context.retainedCritDefense = 0;
        }
        
        Context.retainedNormals -= Context.retainedCritDefense;
        Context.retainedNormals -= Context.retainedNormalDefense;

        if (Context.retainedNormals <= 0)
        {
            Context.retainedNormals = 0;
        }
        
        Debug.Log("Remaining crit attacks: " + Context.retainedCrits.ToString());
        Debug.Log("Remaining normal attacks: " + Context.retainedNormals.ToString());
    }

    private void ApplyFinalDamage()
    {
        int totalDamage = 0;
        
        for (int i = 0; i < Context.retainedCrits; i++)
        {
            totalDamage += Context.weapon.DMGcrit;
        }

        for (int i = 0; i < Context.retainedNormals; i++)
        {
            totalDamage += Context.weapon.DMGnorm;
        }
        
        Context.currentlySelectedTargetScript.TakeDamage(totalDamage);

        Context.currentlySelectedTargetScript.UpdateHealth(Context.currentlySelectedTargetScript.currentWounds, Context.currentlySelectedTargetScript.operativeData.WOUNDS);
        
    }

    public override void Update()
    {
        //No operation
    }

    public override void OnExit()
    {
        Debug.Log("CombatState Exited");
        //_menu.CloseMenu(_menu.diceRollMenu);
        _diceHandler.ClearAllDice();
        Context.currentlySelectedUnitScript.UpdateAPL(Context.currentlySelectedUnitScript.currentAPL -= 1);
        Context.Reset();
    }
    

    private void ExecuteRulesInThisStep(AttackTimings currentStep)
    {
        List<Action> rulesToExecute = new(); //List of rules that need to be executed from units and weapons

        if (Context.weapon != null)
        {
            foreach (var rule in Context.weapon.rules) //Gather all weapon rules for this step
            {
                if (rule.Step == currentStep)
                {
                    rulesToExecute.Add(() => rule.Execute(Context)); //List of delegate function from the rules
                }
            }
        }

        if (Context.activatedUnitSO != null) //Gather all active abilities for this step
        {
            foreach (var rule in Context.activatedUnitSO.Aabilities)
            {
                if (rule.Step == currentStep)
                {
                    rulesToExecute.Add(() => rule.Execute(Context));
                }
            }
            foreach (var rule in Context.activatedUnitSO.Pabilities)
            {
                if (rule.Step == currentStep)
                    rulesToExecute.Add(() => rule.Execute(Context));
            }
        }
        
        if (Context.targetUnitSO != null) //Gather all passive abilities for this step
        {
            foreach (var rule in Context.targetUnitSO.Aabilities)
            {
                if (rule.Step == currentStep)
                    rulesToExecute.Add(() => rule.Execute(Context));
            }
            foreach (var rule in Context.targetUnitSO.Pabilities)
            {
                if (rule.Step == currentStep)
                    rulesToExecute.Add(() => rule.Execute(Context));
            }
        }
        foreach (var executeAction in rulesToExecute) //Run all of the delegate function held in rulesToExecute
        {
            executeAction.Invoke();
        }
    }
}

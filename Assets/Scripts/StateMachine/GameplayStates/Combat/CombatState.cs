using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----CombatState Class (Handles all of combat)-----
public class CombatState : BaseState
{
    //Inherits from BaseState.
    //The combat state that handles rolling and rules
    
    private DiceHandler _diceHandler; //Needs reference to the CombatManager script on the CombatManager GameObject
    private MenuPanel _menu;

    public CombatState(InformationPackage context, MenuPanel menu, DiceHandler diceHandler) : base(context) //CombatState constructor ": base(context)" is handing context up to the BaseState constructor
    {
        _diceHandler = diceHandler;
        _menu = menu;
    }
    
    public override void OnEnter()
    {
        Debug.Log("CombatState entered");
        
        Context.DiceHandler = _diceHandler;
        
        //Reset
        Context.AttackRolls.Clear();
        Context.DefenseRolls.Clear();
        Context.RetainedCrits = 0;
        Context.RetainedNormals = 0;
        Context.RetainedCritDefense = 0;
        Context.RetainedNormalDefense = 0;

        //tutorial panel flags
        _menu.tutFlag7B = true; //remove later
        if(_menu.tutFlag5)
        {
            _menu.tutFlag5 = false;
        }
        if(_menu.tutFlag4)
        {
            _menu.tutFlag4 = false;
        }
        
        _diceHandler.StartCoroutine(ResolveCombat());
    }

    private IEnumerator ResolveCombat()
    {
        Context.CurrentlySelectedUnitScript.hasShotThisActivation = true;
        Debug.Log("ResolveCombat");
        
        //Pre-Roll
        ExecuteRulesInThisStep(AttackTimings.PreRoll);
        
        //Roll Attack Dice and defense dice
        yield return _diceHandler.StartCoroutine(_diceHandler.ThrowAttackDice(Context.Weapon.atk, true, Context));
        
        //After attack roll
        ExecuteRulesInThisStep(AttackTimings.AfterAttackRoll);
        
        //Rerolls
        
        yield return new WaitForSeconds(2f);

        yield return _diceHandler.StartCoroutine(_diceHandler.ThrowDefenseDice(Context.NumDefenseDiceRoll, true, Context));
        
        //Attack Evaluation
        ExecuteRulesInThisStep(AttackTimings.AttackEvaluation);
        EvaluateHitsAndSaves();
        
        //After Attack Evaluation
        ExecuteRulesInThisStep(AttackTimings.AfterAttackEvaluation);
        ApplyFinalDamage();
        
        yield return new WaitForSeconds(1.5f);
        _diceHandler.ClearDiceRolls();
        Context.CurrentlySelectedUnitScript.lineRenderer.enabled = false;
        Context.IsShootingConfirmed = true;
    }
    
    private void EvaluateHitsAndSaves()
    {
        Debug.Log("Evaluating hits and saves");
        for (int i = 0; i < Context.AttackRolls.Count; i++)
        {
            if (Context.AttackRolls[i] >= Context.Weapon.hit)
            {
                if (Context.AttackRolls[i] == 6)
                {
                    Context.RetainedCrits += 1;
                    Debug.Log("Number of crits: " + Context.RetainedCrits.ToString());
                }
                else
                {
                    Context.RetainedNormals += 1;
                    Debug.Log("Number of normals: " + Context.RetainedNormals.ToString());
                }
            }
        }

        for (int i = 0; i < Context.DefenseRolls.Count; i++)
        {
            if (Context.DefenseRolls[i] >= Context.TargetUnitSo.save)
            {
                if (Context.DefenseRolls[i] == 6)
                {
                    Context.RetainedCritDefense += 1;
                    Debug.Log("Number of crit saves: " + Context.RetainedCritDefense.ToString());
                }
                else
                {
                    Context.RetainedNormalDefense += 1;
                    Debug.Log("Number of normals saves: " + Context.RetainedNormalDefense.ToString());
                }
            }
        }

        if (Context.RetainedCritDefense >= Context.RetainedCrits)
        {
            Context.RetainedCritDefense -= Context.RetainedCrits;
            Context.RetainedCrits = 0;
        } else if (Context.RetainedCritDefense < Context.RetainedCrits)
        {
            Context.RetainedCrits -= Context.RetainedCritDefense;
            Context.RetainedCritDefense = 0;
        }
        
        Context.RetainedNormals -= Context.RetainedCritDefense;
        Context.RetainedNormals -= Context.RetainedNormalDefense;

        if (Context.RetainedNormals <= 0)
        {
            Context.RetainedNormals = 0;
        }
        
        Debug.Log("Remaining crit attacks: " + Context.RetainedCrits.ToString());
        Debug.Log("Remaining normal attacks: " + Context.RetainedNormals.ToString());
    }

    private void ApplyFinalDamage()
    {
        int totalDamage = 0;
        
        for (int i = 0; i < Context.RetainedCrits; i++)
        {
            totalDamage += Context.Weapon.dmgCrit;
        }

        for (int i = 0; i < Context.RetainedNormals; i++)
        {
            totalDamage += Context.Weapon.dmgNorm;
        }
        
        Context.CurrentlySelectedTargetScript.TakeDamage(totalDamage);

        Context.CurrentlySelectedTargetScript.UpdateHealth(Context.CurrentlySelectedTargetScript.currentWounds, Context.CurrentlySelectedTargetScript.operativeData.wounds);
        
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
        Context.CurrentlySelectedUnitScript.UpdateApl(Context.CurrentlySelectedUnitScript.currentApl -= 1);
    }
    
    private void ExecuteRulesInThisStep(AttackTimings currentStep)
    {
        List<Action> rulesToExecute = new(); //List of rules that need to be executed from units and weapons

        if (Context.Weapon != null)
        {
            foreach (var rule in Context.Weapon.rules) //Gather all weapon rules for this step
            {
                if (rule.Step == currentStep)
                {
                    rulesToExecute.Add(() => rule.Execute(Context)); //List of delegate function from the rules
                }
            }
        }

        if (Context.ActivatedUnitSo != null) //Gather all active abilities for this step
        {
            foreach (var rule in Context.ActivatedUnitSo.aabilities)
            {
                if (rule.Step == currentStep)
                {
                    rulesToExecute.Add(() => rule.Execute(Context));
                }
            }
            foreach (var rule in Context.ActivatedUnitSo.pabilities)
            {
                if (rule.Step == currentStep)
                    rulesToExecute.Add(() => rule.Execute(Context));
            }
        }
        
        if (Context.TargetUnitSo != null) //Gather all passive abilities for this step
        {
            foreach (var rule in Context.TargetUnitSo.aabilities)
            {
                if (rule.Step == currentStep)
                    rulesToExecute.Add(() => rule.Execute(Context));
            }
            foreach (var rule in Context.TargetUnitSo.pabilities)
            {
                if (rule.Step == currentStep)
                    rulesToExecute.Add(() => rule.Execute(Context));
            }
        }
        foreach (var executeAction in rulesToExecute) //Run all the delegate functions held in rulesToExecute
        {
            executeAction.Invoke();
        }
    }
}

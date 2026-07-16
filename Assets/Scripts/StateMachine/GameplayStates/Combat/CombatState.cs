using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CombatState : BaseState
{
    //Inherits from BaseState.
    //The combat state that handles rolling and rules
    
    public CombatManager _combatManager; //Needs reference to the CombatManager script on the CombatManager GameObject

    public CombatState(InformationPackage context, CombatManager combatManager) : base(context) //CombatState constructor ": base(context)" is handing context up to the BaseState constructor
    {
        _combatManager = combatManager;
    }
    
    public override void OnEnter()
    {
        Debug.Log("CombatState entered");
        _combatManager.StartCoroutine(ResolveCombat());
    }

    private IEnumerator ResolveCombat()
    {
        Debug.Log("ResolveCombat");
        
        //Pre-Roll
        ExecuteRulesInThisStep(AttackTimings.PreRoll);
        
        //Roll Attack Dice
        RollAttackDice();
        _combatManager.SpawnDice(Context.attackRolls, isAttack: true);
        yield return new WaitForSeconds(2.5f);
        
        //Roll Defense Dice
        RollDefenseDice();
        _combatManager.SpawnDice(Context.defenseRolls, isAttack: false);
        yield return new WaitForSeconds(2.5f);
        
        //After-Roll
        //Keep track of values before rerolls
        List<int> oldAttackRolls = new List<int>(Context.attackRolls);
        List<int> oldDefenseRolls = new List<int>(Context.defenseRolls);
        ExecuteRulesInThisStep(AttackTimings.AfterRoll);
        
        //Check for rerolls and rigger reroll animations if values have changed
        bool attackRerolled = false;
        for (int i = 0; i < Context.attackRolls.Count; i++)
        {
            if (Context.attackRolls[i] != oldAttackRolls[i])
            {
                _combatManager.RerollDieVisually(i, Context.attackRolls[i], isAttack: true);
                attackRerolled = true;
            }
        }

        bool defenseRerolled = false;
        for (int i = 0; i < Context.defenseRolls.Count; i++)
        {
            if (Context.defenseRolls[i] != oldDefenseRolls[i])
            {
                _combatManager.RerollDieVisually(i, Context.defenseRolls[i], isAttack: false);
                defenseRerolled = true;
            }
        }

        // If rules changed rolls, pause the state execution for the reroll animations
        if (attackRerolled || defenseRerolled)
        {
            yield return new WaitForSeconds(2.5f);
        }
        
        //Attack Evaluation
        ExecuteRulesInThisStep(AttackTimings.AttackEvaluation);
        EvaluateHitsAndSaves();
        
        //After Attack Evaluation
        ExecuteRulesInThisStep(AttackTimings.AfterAttackEvaluation);
        ApplyFinalDamage();
        
        yield return new WaitForSeconds(1.5f);
        _combatManager.ClearAllDice();
        Context.isShootingConfirmed = true;
    }

    private void RollAttackDice()
    {
        Debug.Log("Rolling attack dice");
        int totalDice = Context.weapon.ATK;
        for (int i = 0; i < totalDice; i++)
        {
            Context.attackRolls.Add(Random.Range(1,7));
        }
        
        //Debugging test
        for (int i = 0; i < Context.attackRolls.Count; i++)
        {
            Debug.Log(Context.attackRolls[i].ToString());
        }
    }

    private void RollDefenseDice()
    {
        Debug.Log("Rolling defense dice");
        int totalDice = 3; //Base 3 defence dice
        for (int i = 0; i < totalDice; i++)
        {
            Context.defenseRolls.Add(Random.Range(1,7));
        }
        
        //Debugging test
        for (int i = 0; i < Context.defenseRolls.Count; i++)
        {
            Debug.Log(Context.defenseRolls[i].ToString());
        }
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
        
    }

    public override void Update()
    {
        //No operation
    }

    public override void OnExit()
    {
        Debug.Log("CombatState Exited");
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

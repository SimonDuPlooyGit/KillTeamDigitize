using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//-----CombatState Class (Handles all of combat)-----
public class ShootCombatState : BaseState
{
    //Inherits from BaseState.
    //The combat state that handles rolling and rules
    
    private DiceHandler _diceHandler; //Needs reference to the CombatManager script on the CombatManager GameObject
    private MenuPanel _menu;

    public ShootCombatState(InformationPackage context, MenuPanel menu, DiceHandler diceHandler) : base(context) //CombatState constructor ": base(context)" is handing context up to the BaseState constructor
    {
        _diceHandler = diceHandler;
        _menu = menu;
    }
    
    public override void OnEnter()
    {
        Debug.Log("ShootCombatState OnEnter");
        
        Context.DiceHandler = _diceHandler;
        
        //Reset
        Context.AttackRolls.Clear();
        Context.DefenseRolls.Clear();
        Context.RetainedCrits = 0;
        Context.RetainedNormals = 0;
        Context.RetainedCritDefense = 0;
        Context.RetainedNormalDefense = 0;
        
        _diceHandler.StartCoroutine(ResolveCombat());
        Context.CurrentlySelectedTargetScript.ForceUIOn(true);

        var silentRule = Context.Weapon.rules.OfType<WeaponRules.Silent>().FirstOrDefault();
        if (silentRule != null)
        {
            Debug.Log("Silent rule on weapon, can shoot from conceal");
        }
        else
        {
            Debug.Log("Firing non silent weapon, order changing to Engage");
            Context.CurrentlySelectedOperativeScript.currentOrder = OrderState.Engage;
        }
    }

    private IEnumerator ResolveCombat()
    {
        var playerOp = Context.CurrentlySelectedOperativeScript as PlayerOperative;
        if (playerOp != null) playerOp.hasShotThisActivation = true;

        var heavyRule = Context.Weapon.rules.OfType<WeaponRules.Heavy>().FirstOrDefault();
        if (heavyRule != null)
        {
            playerOp.hasRepositionedThisActivation = true; //Take reposition away if you start a shoot with a heavy weapon
            playerOp.hasChargedThisActivation = true; //Take charge away if you have started a shoot witha heavy weapon
        }
        Debug.Log("ResolveCombat");
        
        Context.MainCameraScript.SetOrbitTarget(Context.CurrentlySelectedTarget.transform);
        
        if (Context.CurrentlySelectedOperativeScript.lineRenderer != null)
        {
            LineRenderer lr = Context.CurrentlySelectedOperativeScript.lineRenderer;
            lr.positionCount = 2;
            lr.SetPosition(0, Context.CurrentlySelectedOperativeScript.losStart.position);
            lr.SetPosition(1, Context.CurrentlySelectedTargetScript.losStart.position);
            lr.material = Context.CurrentlySelectedOperativeScript.validLine;
            lr.enabled = true;
        }
        
        if (Context.CurrentlySelectedTargetScript.currentOrder == OrderState.Conceal &&
            Context.CurrentlySelectedTargetScript.closeToCover)
        {
            Context.CurrentlySelectedOperativeScript.CheckTargetingLines(
                Context.CurrentlySelectedOperativeScript, Context.CurrentlySelectedTargetScript);
        }
        
        yield return new WaitForSeconds(2.0f);
        
        //Pre-Roll
        ExecuteRulesInThisStep(AttackTimings.PreRoll);
        
        //Roll Attack Dice and defense dice
        Context.MainCameraScript.SetOrbitTarget(Context.CurrentlySelectedOperative.transform);
        yield return _diceHandler.StartCoroutine(_diceHandler.ThrowAttackDice(Context.Weapon.atk, true, Context));
        
        //After attack roll
        ExecuteRulesInThisStep(AttackTimings.AfterAttackRoll);
        
        //Rerolls
        
        yield return new WaitForSeconds(2f);

        Context.MainCameraScript.SetOrbitTarget(Context.CurrentlySelectedTarget.transform);
        yield return _diceHandler.StartCoroutine(_diceHandler.ThrowDefenseDice(Context.NumDefenseDiceRoll, true, Context));
        
        //Attack Evaluation
        ExecuteRulesInThisStep(AttackTimings.AttackEvaluation);
        EvaluateHitsAndSaves();
        
        //After Attack Evaluation
        ExecuteRulesInThisStep(AttackTimings.AfterAttackEvaluation);
        ApplyFinalDamage();
        
        yield return new WaitForSeconds(1.5f);
        Context.MainCameraScript.SetOrbitTarget(Context.CurrentlySelectedOperative.transform);
        _diceHandler.ClearDiceRolls();
        Context.CurrentlySelectedOperativeScript.lineRenderer.enabled = false;
        Context.IsShootingConfirmed = true;
    }
    
    private void EvaluateHitsAndSaves()
    {
        Debug.Log("Evaluating hits and saves");
        for (int i = 0; i < Context.AttackRolls.Count; i++)
        {
            if (Context.CurrentlySelectedOperativeScript.injured)
            {
                if (Context.AttackRolls[i] >= Context.Weapon.hit + 1)
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
            else
            {
                if (Context.AttackRolls[i] >= Context.Weapon.hit)
                {
                    if (Context.AttackRolls[i] == 6)
                    {
                        Context.RetainedCrits += 1;
                        Debug.Log("Number of crits: " + Context.RetainedCrits);

                        if (Context.RetainedCrits > 0)
                        {
                            //Debug.LogWarning($"{Context.CurrentlySelectedOperative.name} rolled {Context.RetainedCrits} crits for {Context.Weapon.dmgCrit * Context.RetainedCrits} total dmg");
                        }
                    }
                    else
                    {
                        Context.RetainedNormals += 1;
                        Debug.Log("Number of normals: " + Context.RetainedNormals);
                        
                        if (Context.RetainedNormals > 0)
                        {
                            //Debug.LogWarning($"{Context.CurrentlySelectedOperative.name} rolled {Context.RetainedNormals} normals for {Context.Weapon.dmgNorm * Context.RetainedNormals} total dmg");
                        }
                    }
                }
            }
        }

        for (int i = 0; i < Context.DefenseRolls.Count; i++)
        {
            if (Context.DefenseRolls[i] >= Context.CurrentlySelectedTargetSo.save)
            {
                if (Context.DefenseRolls[i] == 6)
                {
                    Context.RetainedCritDefense += 1;
                    Debug.Log("Number of crit saves: " + Context.RetainedCritDefense);

                    if (Context.RetainedCritDefense > 0)
                    {
                        //Debug.LogWarning($"{Context.CurrentlySelectedTarget.name} rolled {Context.RetainedCritDefense} crit defenses");
                    }
                }
                else
                {
                    Context.RetainedNormalDefense += 1;
                    Debug.Log("Number of normals saves: " + Context.RetainedNormalDefense);
                    
                    if (Context.RetainedNormalDefense > 0)
                    {
                        //Debug.LogWarning($"{Context.CurrentlySelectedTarget.name} rolled {Context.RetainedNormalDefense} normal defenses");
                    }
                }
            }
        }

        SaveResolver.Resolve(Context.RetainedCrits, Context.RetainedNormals, Context.RetainedCritDefense, Context.RetainedNormalDefense, Context.Weapon.dmgNorm, Context.Weapon.dmgCrit, out int critsLeft, out int normalsLeft);
        
        Context.RetainedCrits = critsLeft;
        Context.RetainedNormals = normalsLeft;

        SoundManager.PlaySound(11,0.7f,true);
        Context.CurrentlySelectedOperativeScript.GetComponent<SmokeTrails>().SpawnSmoke(Context.RetainedNormals,Context.CurrentlySelectedOperative.transform, Context.CurrentlySelectedTarget.transform);
        Context.CurrentlySelectedOperativeScript.GetComponent<SmokeTrails>().SpawnSmoke(Context.RetainedCrits, Context.CurrentlySelectedOperative.transform, Context.CurrentlySelectedTarget.transform);
        Context.CurrentlySelectedOperative.GetComponent<Animator>().SetTrigger("Shoot");

        Debug.Log("Remaining crit attacks: " + Context.RetainedCrits);
        Debug.Log("Remaining normal attacks: " + Context.RetainedNormals);

        if (Context.RetainedCrits > 0)
        {
            //Debug.LogWarning($"{Context.RetainedCrits} crits pass defenses");
        }

        if (Context.RetainedNormals > 0)
        {
            //Debug.LogWarning($"{Context.RetainedNormals} normals pass defenses");
        }
    }

    private void ApplyFinalDamage()
    {
        int totalDamage = 0;
        
        for (int i = 0; i < Context.RetainedCrits; i++)
        {
            totalDamage += Context.Weapon.dmgCrit;
            //Debug.LogWarning($"{Context.CurrentlySelectedOperativeScript.name} dealt {Context.Weapon.dmgCrit} crit damage to {Context.CurrentlySelectedTarget.name}");
        }

        for (int i = 0; i < Context.RetainedNormals; i++)
        {
            totalDamage += Context.Weapon.dmgNorm;
            //Debug.LogWarning($"{Context.CurrentlySelectedOperativeScript.name} dealt {Context.Weapon.dmgCrit} crit damage to {Context.CurrentlySelectedTarget.name}");
        }
        
        Context.CurrentlySelectedTargetScript.TakeDamage(totalDamage);
        Debug.LogWarning($"{Context.CurrentlySelectedOperativeScript.name} shot {Context.CurrentlySelectedTarget.name} for {totalDamage} dmg");
        Context.CurrentlySelectedTargetScript.UpdateHealth(Context.CurrentlySelectedTargetScript.currentWounds, Context.CurrentlySelectedTargetScript.operativeData.wounds);
        
    }

    public override void Update()
    {
        //No operation
    }

    public override void OnExit()
    {
        Debug.Log("ShootCombatState OnExit");
        //_menu.CloseMenu(_menu.diceRollMenu);
        _diceHandler.ClearAllDice();
        Context.CurrentlySelectedOperativeScript.UpdateApl(1);
        Context.CurrentlySelectedTargetScript.ForceUIOn(false);
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

        if (Context.CurrentlySelectedOperativeSo != null) //Gather all active abilities for this step
        {
            foreach (var rule in Context.CurrentlySelectedOperativeSo.aabilities)
            {
                if (rule.Step == currentStep)
                {
                    rulesToExecute.Add(() => rule.Execute(Context));
                }
            }
            foreach (var rule in Context.CurrentlySelectedOperativeSo.pabilities)
            {
                if (rule.Step == currentStep)
                    rulesToExecute.Add(() => rule.Execute(Context));
            }
        }
        
        if (Context.CurrentlySelectedTargetSo != null) //Gather all passive abilities for this step
        {
            foreach (var rule in Context.CurrentlySelectedTargetSo.aabilities)
            {
                if (rule.Step == currentStep)
                    rulesToExecute.Add(() => rule.Execute(Context));
            }
            foreach (var rule in Context.CurrentlySelectedTargetSo.pabilities)
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

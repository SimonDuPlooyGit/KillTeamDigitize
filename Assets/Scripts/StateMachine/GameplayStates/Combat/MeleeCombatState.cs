using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using static DiceHandler;

public class MeleeCombatState : BaseState
{
    private MenuPanel _menu;
    private DiceHandler _diceHandler;
    private WeaponTemplate attackerWeapon;
    private WeaponTemplate retaliatorWeapon;
    
    public MeleeCombatState(InformationPackage context, MenuPanel menu, DiceHandler diceHandler) : base(context)
    {
        _menu = menu;
        _diceHandler = diceHandler;
    }

    public override void OnEnter()
    {
        Debug.Log("MeleeCombatState OnEnter");
        
        Context.DiceHandler = _diceHandler;
        attackerWeapon = Context.Weapon;
        
        //Reset
        Context.AttackRolls.Clear();
        Context.DefenseRolls.Clear();
        Context.RetainedCrits = 0;
        Context.RetainedNormals = 0;
        Context.RetainedCritDefense = 0;
        Context.RetainedNormalDefense = 0;

        var playerOp = Context.CurrentlySelectedOperativeScript as PlayerOperative;
        if (playerOp != null) playerOp.hasFoughtThisActivation = true;

        _diceHandler.StartCoroutine(ResolveSequentialFight());
    }

    public override void Update()
    {
        //no op
    }

    public override void OnExit()
    {
        Debug.Log("MeleeCombatState OnExit");
        _diceHandler.ClearAllDice();
        Context.CurrentlySelectedOperativeScript.UpdateApl(Context.CurrentlySelectedOperativeScript.currentApl -= 1);
    }

    //Find the melee weapon for the person defend fighting
    private WeaponTemplate FindRetaliatorMeleeWeapon(BaseOperativeClass targetUnit)
    {
        WeaponTemplate weaponToReturn =
            targetUnit.operativeData.weapons.FirstOrDefault(weapon => weapon.rules.OfType<WeaponRules.Melee>().Any());
        
        return weaponToReturn;
    }
    
    //Resolve parry and attack sequence, player then npo so and until dice done
    //Need to remove dice from UI that have been used
    //If a player picks parry they must be able to select an opponent dice to parry (with crit parry rules applied)
    private IEnumerator ResolveSequentialFight()
    {
        //Player is the attacked if the activated operative is of type PlayerOperative
        bool isPlayerAttacker = Context.CurrentlySelectedOperativeScript is PlayerOperative;
        Context.IsPlayerAttacker = isPlayerAttacker;

        //Throw attack dice for the current attacker
        yield return _diceHandler.StartCoroutine(_diceHandler.ThrowAttackDice(attackerWeapon.atk, isPlayerAttacker, Context));

        if (Context.CurrentlySelectedTargetScript != null)
        {
            retaliatorWeapon = FindRetaliatorMeleeWeapon(Context.CurrentlySelectedTargetScript);
            Context.opponentWeapon = retaliatorWeapon;
            
            //Throw the retaliator's attacks as "Defense dice"
            //Menu selection handling happens in DiceHandler based on state
            yield return _diceHandler.StartCoroutine(_diceHandler.ThrowDefenseDice(attackerWeapon.atk, isPlayerAttacker, Context));
            
            //Find successful dice
            InitializeSuccessPools();

            bool attackerTurn = true;

            //Keep alternating till no dice left
            while (Context.AttackerRemainingDice.Count > 0 || Context.RetaliatorRemainingDice.Count > 0)
            {
                if (Context.CurrentlySelectedOperativeScript == null || Context.CurrentlySelectedOperativeScript.dead ||
                    Context.CurrentlySelectedTargetScript == null || Context.CurrentlySelectedTargetScript.dead)
                {
                    Debug.Log("Unit defeated. Ending melee sequence early");
                    break;
                }
                
                if (attackerTurn && Context.AttackerRemainingDice.Count == 0)
                {
                    attackerTurn = false;
                    continue;
                }
                if (!attackerTurn && Context.RetaliatorRemainingDice.Count == 0)
                {
                    attackerTurn = true;
                    continue;
                }

                bool isPlayerTurn = (attackerTurn == isPlayerAttacker);
                Context.IsPlayerFightTurn = isPlayerTurn;

                if (isPlayerTurn)
                {
                    Debug.Log("Player turn to strike or parry");
                    _diceHandler.playerActionReady = false;
                    _diceHandler.currentSelectionMode = DiceSelectionMode.None;
                    _diceHandler.UpdateRerollButtonVisibility();
                    
                    yield return new WaitUntil(() => _diceHandler.playerActionReady || Context.CurrentlySelectedTargetScript.dead || Context.CurrentlySelectedOperativeScript.dead);

                    _diceHandler.playerActionReady = false;
                }
                else
                {
                    Debug.Log("NPO turn to strike or parry");
                    yield return new WaitForSeconds(1.0f);
                    ProcessNpoChoice(attackerTurn);
                    _diceHandler.ClearAllSelections(); 
                    _diceHandler.UpdateRerollButtonVisibility();
                }
                
                attackerTurn = !attackerTurn;
                yield return new WaitForSeconds(0.5f);
            }
            
            yield return new WaitForSeconds(1.0f);
            _diceHandler.ClearDiceRolls();
            Context.IsFightingRequested = true;
        }
    }

    //Find the successful rolls and populate them into the Context's remaining attacker and retaliator dice
    private void InitializeSuccessPools()
    {
        Context.AttackerRemainingDice.Clear();
        Context.RetaliatorRemainingDice.Clear();

        foreach (int roll in Context.AttackRolls)
        {
            if (roll >= attackerWeapon.hit) Context.AttackerRemainingDice.Add(roll);
        }
        
        foreach (int roll in Context.DefenseRolls)
        {
            if (roll >= retaliatorWeapon.hit) Context.RetaliatorRemainingDice.Add(roll);
        }
    }

    //This is where player must select dice
    private void ProcessPlayerChoice()
    {
        foreach (var dieSprite in _diceHandler.activeAttackDice)
        {
            if (dieSprite.IsSelected)
            {
                int val = dieSprite.GetValue();
                Context.AttackerRemainingDice.Remove(val);
                
                bool choseParry = false; //This needs to be hooked up to dice selection

                if (choseParry)
                {
                    if (val == 6) Context.RetainedCritDefense++;
                    else Context.RetainedNormalDefense++;
                    Debug.Log($"Player chose to parry with {val}");
                }
                else
                {
                    if (val == 6) Context.RetainedCrits++;
                    else Context.RetainedNormals++;

                    ApplySingleStrikeDamage(val);
                    Debug.Log($"Player chose to strike with {val}");
                }

                break;
            }
        }
    }

    //Parry first always then remaining is fighting
    private void ProcessNpoChoice()
    {
        if (Context.RetaliatorRemainingDice.Count == 0) return;
        int chosenDie = Context.RetaliatorRemainingDice[0];
        Context.RetaliatorRemainingDice.RemoveAt(0);
        bool npoParry = Context.RetainedCrits > Context.RetainedCritDefense || Context.RetainedNormals > Context.RetainedNormalDefense;

        if (npoParry)
        {
            if (chosenDie == 6) Context.RetainedCritDefense++;
            else Context.RetainedNormalDefense++;
            Debug.Log($"NPO chose to parry with {chosenDie}");
        }
        else
        {
            int dmg = (chosenDie == 6) ? retaliatorWeapon.dmgCrit : retaliatorWeapon.dmgNorm;
            Context.CurrentlySelectedOperativeScript.TakeDamage(dmg);
            Debug.Log($"NPO chose to strike with {chosenDie}");
        }
    }

    private void ProcessNpoChoice(bool isAttackerTurn)
    {
        bool isNpoAttacker = !Context.IsPlayerAttacker;
        bool npoActingAsAttacker = isAttackerTurn;

        List<int> npoPool = npoActingAsAttacker ? Context.AttackerRemainingDice : Context.RetaliatorRemainingDice;
        List<int> oppPool = npoActingAsAttacker ? Context.RetaliatorRemainingDice : Context.AttackerRemainingDice;

        List<CombatRoll> npoUIList = npoActingAsAttacker ? _diceHandler.activeAttackDice : _diceHandler.activeDefenseDice;
        List<CombatRoll> oppUIList = npoActingAsAttacker ? _diceHandler.activeDefenseDice : _diceHandler.activeAttackDice;

        if (npoPool.Count == 0) return;

        bool didParry = false;
        if (oppPool.Count > 0)
        {
            // Find highest value opponent die
            int bestOppVal = oppPool.Max();

            // Try to parry with valid NPO die
            for (int i = 0; i < npoPool.Count; i++)
            {
                int pVal = npoPool[i];
                bool valid = (pVal == 6) || (pVal < 6 && bestOppVal < 6);
                if (valid)
                {
                    int npoVal = pVal;
                    npoPool.RemoveAt(i);
                    oppPool.Remove(bestOppVal);

                    // Match and destroy NPO UI die
                    CombatRoll npoUiDie = npoUIList.FirstOrDefault(d => d != null && d.GetValue() == npoVal);
                    if (npoUiDie != null)
                    {
                        npoUIList.Remove(npoUiDie);
                        Object.Destroy(npoUiDie.gameObject);
                    }

                    // Match and destroy Opponent UI die
                    CombatRoll oppUiDie = oppUIList.FirstOrDefault(d => d != null && d.GetValue() == bestOppVal);
                    if (oppUiDie != null)
                    {
                        oppUIList.Remove(oppUiDie);
                        Object.Destroy(oppUiDie.gameObject);
                    }

                    Debug.Log($"[MeleeCombatState] NPO parried player die {bestOppVal} with {npoVal}.");
                    didParry = true;
                    break;
                }
            }
        }

        if (!didParry)
        {
            // Execute Strike using highest NPO die
            int bestNpoVal = npoPool.Max();
            npoPool.Remove(bestNpoVal);

            CombatRoll strikeDie = npoUIList.FirstOrDefault(d => d != null && d.GetValue() == bestNpoVal);
            if (strikeDie != null)
            {
                npoUIList.Remove(strikeDie);
                Object.Destroy(strikeDie.gameObject);
            }

            WeaponTemplate npoWeapon = npoActingAsAttacker ? attackerWeapon : retaliatorWeapon;
            int damage = (bestNpoVal == 6) ? npoWeapon.dmgCrit : npoWeapon.dmgNorm;

            BaseOperativeClass targetUnit = isNpoAttacker ? Context.CurrentlySelectedTargetScript : Context.CurrentlySelectedOperativeScript;
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(damage);
                targetUnit.UpdateHealth(targetUnit.currentWounds, targetUnit.operativeData.wounds);
                Debug.Log($"[MeleeCombatState] NPO struck target with die {bestNpoVal} for {damage} damage.");
            }
        }
    }

    //Apply the damage of one die
    private void ApplySingleStrikeDamage(int dieValue)
    {
        int damage = (dieValue == 6) ? Context.Weapon.dmgCrit : Context.Weapon.dmgNorm;
        if (Context.CurrentlySelectedTargetScript != null)
        {
            Context.CurrentlySelectedTargetScript.TakeDamage(damage);
            Context.CurrentlySelectedTargetScript.UpdateHealth(Context.CurrentlySelectedTargetScript.currentWounds,
                Context.CurrentlySelectedTargetScript.operativeData.wounds);
        }
    }
}

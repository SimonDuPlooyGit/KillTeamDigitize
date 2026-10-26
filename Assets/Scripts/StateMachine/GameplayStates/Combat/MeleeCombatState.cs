using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class MeleeCombatState : BaseState
{
    private MenuPanel _menu;
    private DiceHandler _diceHandler;
    private WeaponTemplate targetsWeapon;
    
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

    private WeaponTemplate FindDefenderMeleeWeapon(BaseOperativeClass targetUnit)
    {
        WeaponTemplate weaponToReturn =
            targetUnit.operativeData.weapons.FirstOrDefault(weapon => weapon.rules.OfType<WeaponRules.Melee>().Any());

        return weaponToReturn;
    }
    
    private IEnumerator ResolveSequentialFight()
    {
        bool isPlayerAttacker = Context.CurrentlySelectedOperativeScript is PlayerOperative;
        yield return _diceHandler.StartCoroutine(_diceHandler.ThrowAttackDice(Context.Weapon.atk, isPlayerAttacker, Context));

        if (Context.CurrentlySelectedTargetScript != null)
        {
            targetsWeapon = FindDefenderMeleeWeapon(Context.CurrentlySelectedTargetScript);
            yield return _diceHandler.StartCoroutine(_diceHandler.ThrowDefenseDice(targetsWeapon.atk, !isPlayerAttacker, Context));
            InitializeSuccessPools();
            Context.IsPlayerFightTurn = isPlayerAttacker;

            while (Context.AttackerRemainingDice.Count > 0 || Context.DefenderRemainingDice.Count > 0)
            {
                if (Context.IsPlayerFightTurn)
                {
                    if (Context.AttackerRemainingDice.Count == 0)
                    {
                        Context.IsPlayerFightTurn = false;
                        continue;
                    }
                    
                    Debug.Log("Player selects parry or melee for dice");
                    _diceHandler.currentSelectionMode = DiceHandler.DiceSelectionMode.Attack;
                    _menu.OpenMenu(_menu.diceRollMenu);
                    _diceHandler.maxAllowedSelections = 1;
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                    ProcessPlayerChoice();
                    _menu.CloseMenu(_menu.diceRollMenu);
                    Context.IsPlayerFightTurn = false;
                }
                else
                {
                    if (Context.DefenderRemainingDice.Count == 0)
                    {
                        Context.IsPlayerFightTurn = true;
                        continue;
                    }
                    
                    Debug.Log("NPO selects parry or melee for dice");
                    yield return new WaitForSeconds(1.0f);
                    ProcessNpoChoice();
                    Context.IsPlayerFightTurn = true;
                }
                
                yield return null;
            }

            yield return new WaitForSeconds(1.0f);
            _diceHandler.ClearDiceRolls();
            Context.IsFightingRequested = true;
        }
    }

    private void InitializeSuccessPools()
    {
        Context.AttackerRemainingDice.Clear();
        Context.DefenderRemainingDice.Clear();

        foreach (int roll in Context.AttackRolls)
        {
            if (roll >= Context.Weapon.hit) Context.AttackerRemainingDice.Add(roll);
        }

        int defenderHitStat = targetsWeapon.hit;
        foreach (int roll in Context.DefenseRolls)
        {
            if (roll >= defenderHitStat) Context.DefenderRemainingDice.Add(roll);
        }
    }

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

    private void ProcessNpoChoice()
    {
        if (Context.DefenderRemainingDice.Count == 0) return;
        int chosenDie = Context.DefenderRemainingDice[0];
        Context.DefenderRemainingDice.RemoveAt(0);
        bool npoParry = Context.RetainedCrits > Context.RetainedCritDefense || Context.RetainedNormals > Context.RetainedNormalDefense;

        if (npoParry)
        {
            if (chosenDie == 6) Context.RetainedCritDefense++;
            else Context.RetainedNormalDefense++;
            Debug.Log($"NPO chose to parry with {chosenDie}");
        }
        else
        {
            int dmg = (chosenDie == 6) ? targetsWeapon.dmgCrit : targetsWeapon.dmgNorm;
            Context.CurrentlySelectedOperativeScript.TakeDamage(dmg);
            Debug.Log($"NPO chose to strike with {chosenDie}");
        }
    }

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

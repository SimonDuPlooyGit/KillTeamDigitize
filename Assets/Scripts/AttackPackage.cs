using UnityEngine;

public class AttackPackage
{
    // public Unit attacker;
    // public Unit defender;
    // public Weapon weapon;
    //
    // public List<int> attackRolls = new();
    // public List<int> defenseRolls = new();
    //
    // public int retainedSuccesses;
    // public int retainedCrits;
    //
    // public bool canReroll;
    // public bool ignoreCover;
}

public enum GameLoopTiming
{
    PreRoll,
    AfterRoll,
    AttackEvaluation,
    AfterAttackEvaluation
}
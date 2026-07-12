using System.Collections.Generic;
using UnityEngine;

public class InformationPackage
{
    public OperativeTemplate activatedUnit;
    public OperativeTemplate targetUnit;
    public WeaponTemplate weapon;
    
    public List<int> attackRolls = new();
    public List<int> defenseRolls = new();
    
    public int retainedSuccesses;
    public int retainedCrits;
    
    public bool canReroll;
    public bool ignoreCover;
}

public enum AttackTimings
{
    PreRoll,
    AfterRoll,
    AttackEvaluation,
    AfterAttackEvaluation,
}
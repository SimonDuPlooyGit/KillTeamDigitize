using System.Collections.Generic;
using UnityEngine;

public class InformationPackage
{
    //Game Object references
    public GameObject currentlySelectedOperative;
    public PrototypeUnit activePrototypeUnit;
    
    //Data for rules
    public OperativeTemplate activatedUnit;
    public OperativeTemplate targetUnit;
    public WeaponTemplate weapon;
    
    public List<int> attackRolls = new();
    public List<int> defenseRolls = new();
    
    public int retainedSuccesses;
    public int retainedCrits;
    
    public bool canReroll;
    public bool ignoreCover;
    
    //Flags for state completion
    public bool isActionSelectionRequested;
    public bool isMovementConfirmed;
    
    public void Reset()
    {
        currentlySelectedOperative = null;
        activePrototypeUnit = null;
        activatedUnit = null;
        targetUnit = null;
        weapon = null;
        attackRolls.Clear();
        defenseRolls.Clear();
        retainedSuccesses = 0;
        retainedCrits = 0;
        canReroll = false;
        ignoreCover = false;
        isActionSelectionRequested = false;
        isMovementConfirmed = false;

    }
}

public enum AttackTimings
{
    PreRoll,
    AfterRoll,
    AttackEvaluation,
    AfterAttackEvaluation,
}
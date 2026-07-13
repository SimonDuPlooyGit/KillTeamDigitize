using System.Collections.Generic;
using UnityEngine;

public class InformationPackage
{
    //Game Object references
    public GameObject currentlySelectedOperative;
    public PrototypeUnit activePrototypeUnit;
    
    //Data for rules
    public OperativeTemplate activatedUnit;
    public GameObject targetUnit;
    public WeaponTemplate weapon;
    
    public List<int> attackRolls = new();
    public List<int> defenseRolls = new();
    
    public int retainedSuccesses;
    public int retainedCrits;
    
    public bool canReroll;
    public bool ignoreCover;
    
    //Flags for state completion
    public bool isMovementRequested;
    public bool isMovementConfirmed;
    public bool isShootingRequested;
    public bool isShootingConfirmed;
    public bool isWeaponSelected;

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
        isMovementRequested = false;
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
using System.Collections.Generic;
using UnityEngine;

//-----InformationPackage Class (Holds all the information that needs be kept/sent between states)-----
public class InformationPackage
{
    //Game Object references
    public GameObject CurrentlySelectedUnit;
    public PrototypeUnit CurrentlySelectedUnitScript;
    public GameObject CurrentlySelectedTarget;
    public BaseOperativeClass CurrentlySelectedTargetScript;

    //New assignments to differentiate when an NPO is selected and choosing targets
    public PrototypeNPO CurrentlyActivatedNpo;
    
    public DiceHandler DiceHandler;
    public OrbitCamera MainCameraScript;
    
    //Data for rules
    public OperativeTemplate ActivatedUnitSo;
    public OperativeTemplate TargetUnitSo;
    public WeaponTemplate Weapon;
    
    public List<int> AttackRolls = new();
    public List<int> DefenseRolls = new();
    
    public int RetainedNormals;
    public int RetainedCrits;
    public int RetainedCritDefense;
    public int RetainedNormalDefense;

    public int NumDefenseDiceRoll = 3;
    
    public bool CanReroll;
    public bool IgnoreCover;
    
    //Flags for state completion
    public bool IsPlayerActivationRequested;
    public bool IsMovementConfirmed;
    public bool IsShootingRequested;
    public bool IsShootingConfirmed;
    public bool IsWeaponSelected;
    public bool ValidTarget;
    
    public void Reset()
    {
        CurrentlySelectedUnit = null;
        CurrentlySelectedUnitScript = null;
        CurrentlySelectedTarget = null;
        CurrentlySelectedTargetScript = null;
        ActivatedUnitSo = null;
        TargetUnitSo = null;
        Weapon = null;
        AttackRolls.Clear();
        DefenseRolls.Clear();
        RetainedNormals = 0;
        RetainedCrits = 0;
        CanReroll = false;
        IgnoreCover = false;
        IsPlayerActivationRequested = false;
        IsMovementConfirmed = false;
        IsShootingRequested = false;
        IsShootingConfirmed = false;
        IsWeaponSelected = false;
        ValidTarget = false;
        NumDefenseDiceRoll = 3;
    }
}

public enum AttackTimings
{
    PreRoll,
    AfterAttackRoll,
    AfterDefenseRoll,
    AttackEvaluation,
    AfterAttackEvaluation,
}
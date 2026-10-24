using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Need to know what kind of movement the player wants to make
public enum PlayerMovementType {None, Dash, Reposition, Charge}

//-----InformationPackage Class (Holds all the information that needs be kept/sent between states)-----
public class InformationPackage
{
    //Game Object references
    public GameObject CurrentlySelectedOperative;
    public BaseOperativeClass CurrentlySelectedOperativeScript; //This needs to get changed to a BaseOperativeClass
    public OperativeTemplate CurrentlySelectedOperativeSo;
    public GameObject CurrentlySelectedTarget;
    public BaseOperativeClass CurrentlySelectedTargetScript;
    public OperativeTemplate CurrentlySelectedTargetSo;
    
    public DiceHandler DiceHandler;
    public OrbitCamera MainCameraScript;
    
    //Data for rules
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
    public bool IsMovementRequested;
    public bool IsMovementConfirmed;
    public bool IsShootingRequested;
    public bool IsShootingConfirmed;
    public bool IsWeaponSelected;
    public bool ValidTarget;
    
    //Player movement types
    public PlayerMovementType SelectedMovement = PlayerMovementType.None;
    
    //Keeping track of all units
    public List<PlayerOperative> AllPlayerUnits = new();
    public List<NonPlayerOperative> AllNpoUnits = new();
    
    //Turning point tracking
    public int CurrentTurningPoint = 1;
    public const int LastTurningPoint = 4;
    public bool PlayerTurn = true;

    //-----Three overloaded methods to be able to set selected operative data no matter which component of it is passed as the argument-----
    public void SetSelectedOperativesData(GameObject selectedOperative)
    {
        CurrentlySelectedOperative = selectedOperative;
        CurrentlySelectedOperativeScript = CurrentlySelectedOperative.GetComponent<PlayerOperative>();
        CurrentlySelectedOperativeSo = CurrentlySelectedOperativeScript.operativeData;
    }

    public void SetSelectedOperativesData(PlayerOperative selectedOperative)
    {
        CurrentlySelectedOperativeScript = selectedOperative;
        CurrentlySelectedOperative = selectedOperative.gameObject;
        CurrentlySelectedOperativeSo = selectedOperative.operativeData;
    }
    
    public void SetSelectedOperativesData(NonPlayerOperative selectedOperative)
    {
        CurrentlySelectedOperativeScript = selectedOperative;
        CurrentlySelectedOperative = selectedOperative.gameObject;
        CurrentlySelectedOperativeSo = selectedOperative.operativeData;
    }

    public void SetSelectedOperativesData(OperativeTemplate selectedOperative)
    {
        CurrentlySelectedOperativeSo = selectedOperative;
    }
    
    //-----Three overloaded methods to be able to set selected target data no matter which component of it is passed as the argument-----
    public void SetSelectedTarget(GameObject selectedTarget)
    {
        CurrentlySelectedTarget = selectedTarget;
        CurrentlySelectedTargetScript = CurrentlySelectedTarget.GetComponent<BaseOperativeClass>();
        CurrentlySelectedTargetSo = CurrentlySelectedTargetScript.operativeData;
    }

    public void SetSelectedTarget(BaseOperativeClass selectedTarget)
    {
        CurrentlySelectedTargetScript = selectedTarget;
        CurrentlySelectedTarget = selectedTarget.gameObject;
        CurrentlySelectedTargetSo = CurrentlySelectedTargetScript.operativeData;
    }

    public void SetSelectedTarget(OperativeTemplate selectedTarget)
    {
        CurrentlySelectedTargetSo = selectedTarget;
    }
    
    //Reset both Operatives data and also state flags
    public void FullReset()
    {
        Debug.Log("InformationPackage FullReset");
        OperativesDataReset();
        StateFlagsReset();
    }
    
    //Only reset Operatives data
    public void OperativesDataReset()
    {
        Debug.Log("InformationPackage OperativesDataReset");
        CurrentlySelectedOperative = null;
        CurrentlySelectedOperativeScript = null;
        CurrentlySelectedOperativeSo = null;
        CurrentlySelectedTarget = null;
        CurrentlySelectedTargetScript = null;
        CurrentlySelectedTargetSo = null;
        Weapon = null;
        AttackRolls.Clear();
        DefenseRolls.Clear();
        RetainedNormals = 0;
        RetainedCrits = 0;
        RetainedCritDefense = 0;
        RetainedNormalDefense = 0;
    }

    //Only reset StateFlags
    public void StateFlagsReset()
    {
        Debug.Log("InformationPackage StateFlagsReset");
        IsPlayerActivationRequested = false;
        IsMovementConfirmed = false;
        IsShootingRequested = false;
        IsShootingConfirmed = false;
        IsWeaponSelected = false;
        ValidTarget = false;
    }
}

//Enum holding timing states for rules
public enum AttackTimings
{
    PreRoll,
    AfterAttackRoll,
    AfterDefenseRoll,
    AttackEvaluation,
    AfterAttackEvaluation,
}
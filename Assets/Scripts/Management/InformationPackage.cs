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
        Debug.Log("SetSelectedOperativesData called (GameObject version)");
        CurrentlySelectedOperative = selectedOperative;
        Debug.Log("CurrentlySelectedOperative is set (GameObject)");
        CurrentlySelectedOperativeScript = CurrentlySelectedOperative.GetComponent<PlayerOperative>();
        Debug.Log("CurrentlySelectedOperativeScript is set (PlayerOperative script)");
        CurrentlySelectedOperativeSo = CurrentlySelectedOperativeScript.operativeData;
        Debug.Log("CurrentlySelectedOperativeSo is set (PlayerOperative's operativeData)");
    }

    public void SetSelectedOperativesData(PlayerOperative selectedOperative)
    {
        Debug.Log("SetSelectedOperativesData called (PlayerOperative version)");
        CurrentlySelectedOperativeScript = selectedOperative;
        Debug.Log("CurrentlySelectedOperativeScript is set (PlayerOperative script)");
        CurrentlySelectedOperative = selectedOperative.gameObject;
        Debug.Log("CurrentlySelectedOperativeScript is set (GameObject)");
        CurrentlySelectedOperativeSo = selectedOperative.operativeData;
        Debug.Log("CurrentlySelectedOperativeSo is set (PlayerOperative's operativeData)");
    }
    
    public void SetSelectedOperativesData(NonPlayerOperative selectedOperative)
    {
        Debug.Log("SetSelectedOperativesData called (NonPlayerOperative version)");
        CurrentlySelectedOperativeScript = selectedOperative;
        Debug.Log("CurrentlySelectedOperativeScript is set (NonPlayerOperative script)");
        CurrentlySelectedOperative = selectedOperative.gameObject;
        Debug.Log("CurrentlySelectedOperativeScript is set (GameObject)");
        CurrentlySelectedOperativeSo = selectedOperative.operativeData;
        Debug.Log("CurrentlySelectedOperativeSo is set (NonPlayerOperative's operativeData)");
    }

    public void SetSelectedOperativesData(OperativeTemplate selectedOperative)
    {
        Debug.Log("SetSelectedOperativesData called (OperativeTemplate version)");
        CurrentlySelectedOperativeSo = selectedOperative;
        Debug.Log("CurrentlySelectedOperativeSo is set (OperativeTemplate SO)");
    }
    
    //-----Three overloaded methods to be able to set selected target data no matter which component of it is passed as the argument-----
    public void SetSelectedTarget(GameObject selectedTarget)
    {
        Debug.Log("SetSelectedTarget called (GameObject version)");
        CurrentlySelectedTarget = selectedTarget;
        CurrentlySelectedTargetScript = CurrentlySelectedTarget.GetComponent<BaseOperativeClass>();
        CurrentlySelectedTargetSo = CurrentlySelectedTargetScript.operativeData;
    }

    public void SetSelectedTarget(BaseOperativeClass selectedTarget)
    {
        Debug.Log("SetSelectedTarget called (BaseOperativeClass version)");
        CurrentlySelectedTargetScript = selectedTarget;
        Debug.Log("CurrentlySelectedTargetScript is set (BaseOperativeClass script)");
        CurrentlySelectedTarget = selectedTarget.gameObject;
        Debug.Log("CurrentlySelectedTarget is set (GameObject)");
        CurrentlySelectedTargetSo = CurrentlySelectedTargetScript.operativeData;
        Debug.Log("CurrentlySelectedTargetSo is set (BaseOperativeClass' operativeData)");
    }

    public void SetSelectedTarget(OperativeTemplate selectedTarget)
    {
        Debug.Log("SetSelectedTarget called (OperativeTemplate version)");
        CurrentlySelectedTargetSo = selectedTarget;
        Debug.Log("CurrentlySelectedTargetSo is set (OperativeTemplate script)");
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
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.SceneManagement;

//-----TargetingState Class (When you have picked a weapon on your unit and you want to click an NPO to shoot)-----
public class TargetingState : BaseState
{
    //Inherits from BaseState
    //State that handles targeting an enemy unit after a Weapon has been selected
    
    private readonly InputActions _input; //Needs to use the input system from GameManager
    private MenuPanel _menu; //Needs access to the menu from GameManager
    //Tutorial flags
    private bool tutFlagTarget = true;
    private bool tutFlagInvalid = true;

    public TargetingState(InformationPackage context, InputActions input, MenuPanel menu) : base(context)
    {
        _input = input;
        _menu = menu;
    }
    
    public override void OnEnter()
    {
        Debug.Log("Targeting State Entered");
        _input.Controls.Select.performed += OnSelectPerformed;

        if (SceneManager.GetActiveScene().name == "Tut2" && tutFlagTarget)
        {
            TutEvents.AdvanceTutorial();
            tutFlagTarget = false;
        }
    }

    public override void Update()
    {
        
    }

    public override void OnExit()
    {
        Debug.Log("Targeting State Exited");
        _input.Controls.Select.performed -= OnSelectPerformed;
        Context.CurrentlySelectedOperativeScript.lineRenderer.material = Context.CurrentlySelectedOperativeScript.validLine;
        Context.CurrentlySelectedOperativeScript.lineRenderer.enabled = false;
    }
    
    //If you left-click shoot a raycast and see if you hit an enemy unit. If so give it to information package
    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        bool validTarget = true;
        bool isInFight = false; //used to distinguish between the types of invalid shoots
        
        Debug.Log("OnSelectPerformed in TargetingState");
        if (!Context.CurrentlySelectedOperativeScript.selected) return;
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            if (hit.collider.gameObject.CompareTag("EnemyUnit")) 
            {
                Context.SetSelectedTarget(hit.collider.gameObject);
                
                //Check if the target is valid for fighting in melee
                if (Context.IsFightingRequested)
                {
                    BaseOperativeClass selectedTargetOp = Context.CurrentlySelectedTargetScript;
                    
                    // Check if the targeted enemy is inside our operative's control range list
                    if (Context.CurrentlySelectedOperativeScript.enemiesInControlRange.Contains(selectedTargetOp))
                    {
                        Context.ValidTargetFight = true;
                        Debug.Log($"Valid melee target selected: {selectedTargetOp.name}");

                        if (SceneManager.GetActiveScene().name == "Tut3")
                        {
                            TutEvents.AdvanceTutorial();
                        }
                    }
                    else
                    {
                        Context.ValidTargetFight = false;
                        Debug.LogWarning($"Target {selectedTargetOp.name} is not within Control Range!");
                    }
                    return;
                }
                
                //Check if the target is valid for shooting since we aren't fighting
                if (Context.CurrentlySelectedTargetScript.currentOrder == OrderState.Conceal &&
                    Context.CurrentlySelectedTargetScript.closeToCover) 
                {
                    Debug.Log("Target is concealed and in cover, checking targeting lines");
                    if (Context.CurrentlySelectedOperativeScript.CheckTargetingLines(
                            Context.CurrentlySelectedOperativeScript, Context.CurrentlySelectedTargetScript))
                    {
                        Debug.Log("Targeting lines rule can see the enemy");
                    }
                    else
                    {
                        Debug.Log("Targeting lines rule can't see the enemy");
                        
                        var seekRule = Context.Weapon != null ? Context.Weapon.rules.OfType<WeaponRules.Seek>().FirstOrDefault() : null;
                        if (seekRule != null)
                        {
                            Debug.Log($"Weapon has Seek rule ({seekRule.coverType}), allowing target selection despite blocked lines.");
                        }
                        else
                        {
                            Debug.Log("Targeting lines rule can't see the enemy and weapon lacks Seek rule.");
                            validTarget = false;
                        }
                    }
                }
                
                float distanceToEnemy = Context.CurrentlySelectedOperativeScript.DetermineLosAndDistance(Context.CurrentlySelectedTargetScript.losStart);
                float maxWeaponRange = Mathf.Infinity;
                var rangeRule = Context.Weapon.rules.OfType<WeaponRules.Range>().FirstOrDefault();
                if (rangeRule != null) maxWeaponRange = rangeRule.range; 
                
                if (distanceToEnemy < maxWeaponRange && distanceToEnemy != 0f)
                {
                    Debug.Log($"Target in range of: {maxWeaponRange}");
                } else if (distanceToEnemy == 0f) //When terrain is hit the distance is zero
                {
                    Debug.Log("Target is not in line of sight");
                    SoundManager.PlaySound(7);
                    _menu.PopUp(_menu.invTarget);
                    validTarget = false;
                }
                else if (distanceToEnemy > maxWeaponRange)
                {
                    Context.CurrentlySelectedOperativeScript.lineRenderer.material = Context.CurrentlySelectedOperativeScript.invalidLine;
                    Debug.Log($"Target out of range of: {maxWeaponRange}");
                    SoundManager.PlaySound(7);
                    _menu.PopUp(_menu.invTarget);
                    validTarget = false;

                    if(SceneManager.GetActiveScene().name == "Tut2" && tutFlagInvalid)
                    {
                        TutEvents.AdvanceTutorial();
                        tutFlagInvalid = false;
                        _menu.tutFlagBack = true;
                    }
                }

                if (Context.CurrentlySelectedTargetScript.EnemyInControlRange)
                {
                    validTarget = false;
                    SoundManager.PlaySound(7);
                    _menu.PopUp(_menu.fightTarget);
                    Debug.Log("Cannot target an operative engaged in a fight");
                    isInFight = true;
                }

                if (validTarget)
                {
                    Context.SetSelectedTarget(hit.collider.gameObject);
                    Debug.Log($"Enemy targeted: {Context.CurrentlySelectedTargetSo.name}");
                
                    //Rotate toward target
                    Context.CurrentlySelectedOperative.transform.LookAt(new Vector3(
                        Context.CurrentlySelectedTarget.transform.position.x, 
                        Context.CurrentlySelectedOperative.transform.position.y, 
                        Context.CurrentlySelectedTarget.transform.position.z));

                    Context.ValidTargetShoot = true;
                }
                else
                {
                    Context.ValidTargetShoot = false;
                    if(!isInFight)
                    {
                        SoundManager.PlaySound(7);
                        _menu.PopUp(_menu.concTarget);
                    }

                    if (SceneManager.GetActiveScene().name == "Tut2" && tutFlagInvalid)
                    {
                        TutEvents.AdvanceTutorial();
                        tutFlagInvalid = false;
                        _menu.tutFlagBack = true;
                    }

                    Debug.Log($"{Context.CurrentlySelectedTargetSo.name} is not a valid shoot target");
                }
            }
        }
    }
    
    /*private void OnSelectPerformed(InputAction.CallbackContext ctx) 
    {
        Debug.Log("OnSelectPerformed in TargetingState");
        if (Context.CurrentlySelectedOperativeScript.selected)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
            {
                if (hit.collider.gameObject.CompareTag("EnemyUnit")) 
                {
                    Context.SetSelectedTarget(hit.collider.gameObject);
                    Debug.Log("Enemy targeted: " + Context.CurrentlySelectedTargetSo.name);

                    Context.CurrentlySelectedOperative.transform.LookAt(new Vector3(Context.CurrentlySelectedTarget.transform.position.x, Context.CurrentlySelectedOperative.transform.position.y, Context.CurrentlySelectedTarget.transform.position.z));
                    
                    if (Context.CurrentlySelectedTargetScript.currentOrder == OrderState.Conceal &&
                        Context.CurrentlySelectedTargetScript.closeToCover) 
                    {
                        Debug.Log("Not a valid target, concealed in cover");
                        return;
                    }
                    
                    float distanceToEnemy = Context.CurrentlySelectedOperativeScript.DetermineLosAndDistance(Context.CurrentlySelectedTargetScript.losStart);
                    float maxWeaponRange = Mathf.Infinity;
                    var rangeRule = Context.Weapon.rules.OfType<WeaponRules.Range>().FirstOrDefault();
                
                    if (rangeRule != null) maxWeaponRange = rangeRule.range; 

                    if (distanceToEnemy < maxWeaponRange && distanceToEnemy != 0f)
                    {
                        Context.ValidTargetShoot = true;
                        Debug.Log($"Target in range of: {maxWeaponRange}");
                    } else if (distanceToEnemy > maxWeaponRange)
                    {
                        Context.ValidTargetShoot = false;
                        Context.CurrentlySelectedOperativeScript.lineRenderer.material = Context.CurrentlySelectedOperativeScript.invalidLine;
                        _menu.tutFlag6 = true; 
                        Debug.Log($"Target out of range of: {maxWeaponRange}");
                    }
                    
                    Context.CurrentlySelectedOperativeScript.CheckTargetingLines(Context.CurrentlySelectedOperativeScript, Context.CurrentlySelectedTargetScript);
                }
            }
        }
    }*/
}
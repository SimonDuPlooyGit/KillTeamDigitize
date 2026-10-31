using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;

//-----TargetingState Class (When you have picked a weapon on your unit and you want to click an NPO to shoot)-----
public class TargetingState : BaseState
{
    //Inherits from BaseState
    //State that handles targeting an enemy unit after a Weapon has been selected
    
    private readonly InputActions _input; //Needs to use the input system from GameManager
    private MenuPanel _menu; //Needs access to the menu from GameManager
    
    public TargetingState(InformationPackage context, InputActions input, MenuPanel menu) : base(context)
    {
        _input = input;
        _menu = menu;
    }
    
    public override void OnEnter()
    {
        Debug.Log("Targeting State Entered");
        _input.Controls.Select.performed += OnSelectPerformed;
        
        //Tutorial panels
        if(_menu.tutFlag5)
        {
            _menu.OpenMenu(_menu.tutTarget);
        }
    }

    public override void Update()
    {
        if(_menu.tutFlag6 && _menu.tutFlag6B)
        {
            _menu.OpenMenu(_menu.tutInvalid);
            _menu.CloseMenu(_menu.tutTarget);
            _menu.tutFlag5 = false;
            _menu.tutFlag4 = true;
        }
        else
        {
            _menu.CloseMenu(_menu.tutInvalid);
        }
    }

    public override void OnExit()
    {
        Debug.Log("Targeting State Exited");
        _input.Controls.Select.performed -= OnSelectPerformed;
        Context.CurrentlySelectedOperativeScript.lineRenderer.material = Context.CurrentlySelectedOperativeScript.validLine;
        Context.CurrentlySelectedOperativeScript.lineRenderer.enabled = false;
        _menu.CloseMenu(_menu.tutTarget);
        _menu.CloseMenu(_menu.tutInvalid);

        //tutorial panels
        if (_menu.tutFlag6)
        {
            _menu.tutFlag6 = false;
            _menu.tutFlag6B = false;
        }
        else if (!_menu.tutFlag6 && _menu.tutFlag7B) //remove later
        {
            _menu.tutFlag7 = true;
        }
    }
    
    //If you left-click shoot a raycast and see if you hit an enemy unit. If so give it to information package
    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        bool validTarget = true;
        
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
                        validTarget = false;
                        
                    }
                }
                
                float distanceToEnemy = Context.CurrentlySelectedOperativeScript.DetermineLosAndDistance(Context.CurrentlySelectedTargetScript.losStart);
                float maxWeaponRange = Mathf.Infinity;
                var rangeRule = Context.Weapon.rules.OfType<WeaponRules.Range>().FirstOrDefault();
                
                if (rangeRule != null) maxWeaponRange = rangeRule.range; 
                
                if (distanceToEnemy < maxWeaponRange && distanceToEnemy != 0f)
                {
                    Debug.Log($"Target in range of: {maxWeaponRange}");
                } 
                else if (distanceToEnemy > maxWeaponRange)
                {
                    Context.CurrentlySelectedOperativeScript.lineRenderer.material = Context.CurrentlySelectedOperativeScript.invalidLine;
                    _menu.tutFlag6 = true; 
                    Debug.Log($"Target out of range of: {maxWeaponRange}");
                    validTarget = false;
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
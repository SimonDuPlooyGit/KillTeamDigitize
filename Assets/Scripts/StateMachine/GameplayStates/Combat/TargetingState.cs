using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
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
        Context.CurrentlySelectedUnitScript.lineRenderer.material = Context.CurrentlySelectedUnitScript.validLine;
        Context.CurrentlySelectedUnitScript.lineRenderer.enabled = false;
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
        Debug.Log("OnSelectPerformed in TargetingState");
        if (Context.CurrentlySelectedUnitScript.selected)
        {
            Debug.Log($"Shooting a targeting raycast for {Context.CurrentlySelectedUnitScript.name}");
            //LOS checking and line renderer code
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
            {
                Debug.Log("Ray fired");
                if (hit.collider.gameObject.CompareTag("EnemyUnit")) //When you hit the enemy check for line of sight and range (called on unit)
                {
                    Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
                    Context.CurrentlySelectedTarget = hit.collider.gameObject;
                    Context.CurrentlySelectedTargetScript = Context.CurrentlySelectedTarget.GetComponent<BaseOperativeClass>();
                    Context.TargetUnitSo = Context.CurrentlySelectedTargetScript.operativeData;
                    Debug.Log("Enemy targeted: " + Context.TargetUnitSo.name);
                    
                    //Cover and conceal checking
                    if (Context.CurrentlySelectedTargetScript.Concealed &&
                        Context.CurrentlySelectedTargetScript.closeToCover) //Concealed and close to cover
                    {
                        Debug.Log("Not a valid target, concealed in cover");
                        return;
                    }
                    
                    float distanceToEnemy = Context.CurrentlySelectedUnitScript.DetermineLosAndDistance(Context.CurrentlySelectedTargetScript.losStart);
                
                    //Assume infinite range first
                    float maxWeaponRange = Mathf.Infinity;
                
                    //Find if the Weapon has the range rule
                    var rangeRule = Context.Weapon.rules.OfType<WeaponRules.Range>().FirstOrDefault();
                
                    //If the Weapon has the range rule assign maxWeaponRange
                    if (rangeRule != null)
                    {
                        // We multiply by your scale/conversion factor if needed, just like meterMovement
                        maxWeaponRange = rangeRule.range; 
                    }

                    if (distanceToEnemy < maxWeaponRange && distanceToEnemy != 0f)
                    {
                        Context.ValidTarget = true;
                        Debug.Log($"Target in range of: {maxWeaponRange}");
                    } else if (distanceToEnemy > maxWeaponRange)
                    {
                        Context.ValidTarget = false;
                        Context.CurrentlySelectedUnitScript.lineRenderer.material =
                        Context.CurrentlySelectedUnitScript.invalidLine;
                        _menu.tutFlag6 = true; //toggle invalid panel
                        Debug.Log($"Target out of range of: {maxWeaponRange}");
                    }
                    
                    //Targeting Lines Checking
                    Context.CurrentlySelectedUnitScript.CheckTargetingLines(Context.CurrentlySelectedUnitScript, Context.CurrentlySelectedTargetScript);
                }
                else
                {
                    Debug.Log($"Didn't hit an npo, raycast hit: {hit.collider.gameObject.name}");
                }
            }
        }
        else if (Context.CurrentlySelectedUnitScript == null)
        {
            Debug.Log("Somehow currently selected unit is null");
        }
        else
        {
            Debug.Log("Somehow currently selected unit is not seen as selected in context");
        }
    }
}
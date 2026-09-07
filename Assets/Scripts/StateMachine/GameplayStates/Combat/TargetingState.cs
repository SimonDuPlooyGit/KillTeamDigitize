using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class TargetingState : BaseState
{
    //Inherits from BaseState
    //State that handles targeting an enemy unit after a weapon has been selected
    
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
        _menu.OpenMenu(_menu.tutTarget);
    }

    public override void Update()
    {
        //no operation
    }

    public override void OnExit()
    {
        Debug.Log("Targeting State Exited");
        _input.Controls.Select.performed -= OnSelectPerformed;
        _menu.CloseMenu(_menu.tutTarget);
    }
    
    private void OnSelectPerformed(InputAction.CallbackContext ctx) //If you left-click shoot a raycast and see if you hit an enemy unit. If so give it to information package
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            if (hit.collider.gameObject.CompareTag("EnemyUnit")) //When you hit the enemy check for line of sight and range (called on unit)
            {
                Context.currentlySelectedTarget = hit.collider.gameObject;
                Context.currentlySelectedTargetScript = Context.currentlySelectedTarget.GetComponent<PrototypeUnit>();
                Context.targetUnitSO = Context.currentlySelectedTargetScript.operativeData;
                Debug.Log("Enemy targeted: " + Context.targetUnitSO.name);
                float distanceToEnemy = Context.currentlySelectedUnitScript.DetermineLOSandDistance(Context.currentlySelectedTargetScript.losStart);
                
                //Assume infinite range first
                float maxWeaponRange = Mathf.Infinity;
                
                //Find if the weapon has the range rule
                var rangeRule = Context.weapon.rules.OfType<WeaponRules.Range>().FirstOrDefault();
                
                //If the weapon has the range rule assign maxWeaponRange
                if (rangeRule != null)
                {
                    // We multiply by your scale/conversion factor if needed, just like meterMovement
                    maxWeaponRange = rangeRule.range; 
                }
                
                if (distanceToEnemy < maxWeaponRange)
                {
                    Context.validTarget = true;
                    Debug.Log($"Target in range of: {maxWeaponRange}");
                } else
                {
                    Context.validTarget = false;
                    Debug.Log($"Target out of range of: {maxWeaponRange}");
                }
            }
        }
    }
}

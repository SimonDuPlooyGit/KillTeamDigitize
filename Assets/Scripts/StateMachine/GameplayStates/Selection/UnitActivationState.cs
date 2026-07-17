using UnityEngine;
using UnityEngine.InputSystem;

public class UnitActivationState : BaseState
{
    //Inherits from BaseState
    //Unit activation state that handles the selection and activations of player operatives
    
    private readonly InputActions _input; //Needs access to the input system from GameManager
    private readonly MenuPanel _menu; //Needs access to the menu from GameManager

    public UnitActivationState(InformationPackage context, InputActions input, MenuPanel menu) : base(context)
    {
        _input = input;
        _menu = menu;
    }
    public override void OnEnter()
    {
        Debug.Log("UnitActivationState entered");
        Context.Reset();
        _input.Controls.Select.performed += OnSelectPerformed;
        _input.Controls.Deselect.performed += OnDeselectPerformed;
    }

    public override void Update()
    {
        //no operation
    }

    public override void OnExit()
    {
        Debug.Log("UnitActivationState exited");
        _input.Controls.Select.performed -= OnSelectPerformed;
        _input.Controls.Deselect.performed -= OnDeselectPerformed;
    }
    
    private void OnSelectPerformed(InputAction.CallbackContext ctx) //If you left-click shoot a raycast and see if hit a friendly unit. If so give it to information package.
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            if (hit.collider.gameObject.CompareTag("AllyUnit"))
            {
                Context.currentlySelectedUnit = hit.collider.gameObject;
                Context.currentlySelectedUnitScript = Context.currentlySelectedUnit.GetComponent<PrototypeUnit>();
                Context.activatedUnitSO = Context.currentlySelectedUnitScript.operativeData;
                Debug.Log("Selected Operative: " + Context.activatedUnitSO.name);
                Context.currentlySelectedUnitScript.selected = true;
                
                _menu.OpenAction();
                Context.isMovementRequested = true; 
            }
        }
    }
    
    private void OnDeselectPerformed(InputAction.CallbackContext ctx) //Deselect the unit that was clicked with right-click
    {
        if (Context.currentlySelectedUnitScript != null)
        {
            Context.currentlySelectedUnitScript.Reset();
            Context.currentlySelectedUnitScript.selected = false;
        }
        Context.Reset();
    }
    
}

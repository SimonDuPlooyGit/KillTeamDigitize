using UnityEngine;
using UnityEngine.InputSystem;

public class UnitActivationState : BaseState
{
    private readonly InputActions _input;
    private readonly MenuPanel _menu;

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
    
    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            if (hit.collider.gameObject.CompareTag("AllyUnit"))
            {
                Context.currentlySelectedOperative = hit.collider.gameObject;
                Context.activePrototypeUnit = Context.currentlySelectedOperative.GetComponent<PrototypeUnit>();
                Context.activePrototypeUnit.selected = true;
                
                _menu.OpenAction();
                Context.isMovementRequested = true; 
            }
        }
    }
    
    private void OnDeselectPerformed(InputAction.CallbackContext ctx)
    {
        if (Context.activePrototypeUnit != null)
        {
            Context.activePrototypeUnit.Reset();
            Context.activePrototypeUnit.selected = false;
        }
        Context.Reset();
        _menu.CloseAction();
    }
    
}

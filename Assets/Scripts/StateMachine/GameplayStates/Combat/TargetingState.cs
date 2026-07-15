using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingState : BaseState
{
    private readonly InputActions _input;
    private readonly MenuPanel _menu;
    
    public TargetingState(InformationPackage context, InputActions input) : base(context)
    {
        _input = input;
    }
    
    public override void OnEnter()
    {
        Debug.Log("Targeting State Entered");
        //_menu.CloseMenu(); What is the parameter supposed to be here?
        //Context.Reset();
        _input.Controls.Select.performed += OnSelectPerformed;
        //_input.Controls.Deselect.performed += OnDeselectPerformed;
    }

    public override void Update()
    {
        //no operation
    }

    public override void OnExit()
    {
        Debug.Log("Targeting State Exited");
        _input.Controls.Select.performed -= OnSelectPerformed;
        //_input.Controls.Deselect.performed -= OnDeselectPerformed;
    }
    
    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            if (hit.collider.gameObject.CompareTag("EnemyUnit"))
            {
                Context.currentlySelectedTarget = hit.collider.gameObject;
                Context.currentlySelectedTargetScript = Context.currentlySelectedTarget.GetComponent<PrototypeUnit>();
                Context.targetUnitSO = Context.currentlySelectedTargetScript.operativeData;
                Debug.Log("Enemy targeted");
            }
        }
    }
}

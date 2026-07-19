using UnityEngine;
using UnityEngine.InputSystem;

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
            if (hit.collider.gameObject.CompareTag("EnemyUnit"))
            {
                Context.currentlySelectedTarget = hit.collider.gameObject;
                Context.currentlySelectedTargetScript = Context.currentlySelectedTarget.GetComponent<PrototypeUnit>();
                Context.targetUnitSO = Context.currentlySelectedTargetScript.operativeData;
                Debug.Log("Enemy targeted: " + Context.targetUnitSO.name);
            }
        }
    }
}

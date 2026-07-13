using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingState : BaseState
{
    private readonly InputActions _input;
    
    public TargetingState(InformationPackage context, InputActions input) : base(context)
    {
        _input = input;
    }
    
    public override void OnEnter()
    {
        Debug.Log("Targeting State Entered");
    }

    public override void Update()
    {
        //no operation
    }

    public override void OnExit()
    {
        Debug.Log("Targeting State Exited");
    }
    
    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            if (hit.collider.gameObject.CompareTag("EnemyUnit"))
            {
                Context.targetUnit = hit.collider.gameObject;
                Debug.Log("Enemy targeted");
            }
        }
    }
}

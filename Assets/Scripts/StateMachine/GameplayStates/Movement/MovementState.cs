using UnityEngine;
using UnityEngine.InputSystem;

public class MovementState : BaseState
{
    private readonly InputActions _input;

    public MovementState(InformationPackage context, InputActions input) : base(context)
    {
        _input = input;
    }

    public override void OnEnter()
    {
        Debug.Log("MovementState entered");
        _input.Controls.Move.performed += OnMoveInputPerformed;
    }

    public override void Update()
    {
        if (Context.activePrototypeUnit != null)
        {
            Context.activePrototypeUnit.UpdatePathDrawing();
        }
    }

    public override void OnExit()
    {
        _input.Controls.Move.performed -= OnMoveInputPerformed;
        Debug.Log("MovementState exited");
    }

    public void OnMoveInputPerformed(InputAction.CallbackContext context)
    {
        if (Context.activePrototypeUnit != null)
        {
            Context.activePrototypeUnit.ClickToPathfind();
        }
    }

    public void ConfirmAndExecuteMovement()
    {
        if (Context.activePrototypeUnit != null)
        {
            Context.activePrototypeUnit.moveUnitToGhost();
            Context.isMovementConfirmed = true;
        }
    }
}

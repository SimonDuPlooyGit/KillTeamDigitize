using UnityEngine;
using UnityEngine.InputSystem;

public class MovementState : BaseState
{
    //Inherits from BaseState
    //Movement state that handles movement of your units (calls the unit's movement functions)
    
    private readonly InputActions _input; //Needs to use the input system from GameManager

    public MovementState(InformationPackage context, InputActions input) : base(context) //Needs base(context) for sending context to the BaseState constructor first
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
        if (Context.currentlySelectedUnitScript != null)
        {
            Context.currentlySelectedUnitScript.UpdatePathDrawing(); //If there is a unit script update its pathdrawing
        }
    }

    public override void OnExit()
    {
        _input.Controls.Move.performed -= OnMoveInputPerformed;
        Debug.Log("MovementState exited");
    }

    public void OnMoveInputPerformed(InputAction.CallbackContext context) //Move the ghost to where you have clicked to move
    {
        if (Context.currentlySelectedUnitScript != null)
        {
            Context.currentlySelectedUnitScript.ClickToPathfind();
        }
    }

    public void ConfirmAndExecuteMovement() //Use the confirm move button to confirm moving the unit to ghost preview
    {
        if (Context.currentlySelectedUnitScript != null)
        {
            Context.currentlySelectedUnitScript.moveUnitToGhost();
            Context.isMovementConfirmed = true;
        }
    }
}

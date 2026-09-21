using UnityEngine;
using UnityEngine.InputSystem;

public class MovementState : BaseState
{
    //Inherits from BaseState
    //Movement state that handles movement of your units (calls the unit's movement functions)
    
    private readonly InputActions _input; //Needs to use the input system from GameManager
    private readonly MenuPanel _menu;
    public bool movementFinished = false;

    public MovementState(InformationPackage context, InputActions input, MenuPanel menu) : base(context) //Needs base(context) for sending context to the BaseState constructor first
    {
        _input = input;
        _menu = menu;
    }

    public override void OnEnter()
    {
        Debug.Log("MovementState entered");
        _input.Controls.Move.performed += OnMoveInputPerformed;
        _menu.OpenMenu(_menu.tutReposition);
        _menu.moveButton2.SetActive(true);
        //You can activate move button here
    }

    public override void Update()
    {
        if (Context.currentlySelectedUnitScript != null)
        {
            Context.currentlySelectedUnitScript.UpdatePathDrawing(); //If there is a unit script update its pathdrawing
        }

        if (Context.currentlySelectedUnitScript != null &&
            Context.currentlySelectedUnitScript.remainingMovement < 0.05f)
        {
            _menu.actionMenu.gameObject.transform.Find("Buttons").transform.Find("MoveButton").gameObject.SetActive(false);
            _menu.actionMenu.gameObject.transform.Find("APLCosts").transform.Find("MoveAPL").gameObject.SetActive(false);
            Context.isMovementRequested = true;
            movementFinished = true;
        }
    }

    public override void OnExit()
    {
        _input.Controls.Move.performed -= OnMoveInputPerformed;
        Debug.Log("MovementState exited");
        _menu.CloseMenu(_menu.tutReposition);
        _menu.moveButton2?.SetActive(false);
        if (Context.currentlySelectedUnitScript.remainingMovement < Context.currentlySelectedUnitScript.meterMovement)
        {
            Context.currentlySelectedUnitScript.UpdateAPL(Context.currentlySelectedUnitScript.currentAPL -= 1);
            _menu.actionMenu.gameObject.transform.Find("Buttons").transform.Find("MoveButton").gameObject.SetActive(false);
            _menu.actionMenu.gameObject.transform.Find("APLCosts").transform.Find("MoveAPL").gameObject.SetActive(false);
        }
        Context.currentlySelectedUnitScript.Reset();
    }

    public void OnMoveInputPerformed(InputAction.CallbackContext ctx) //Move the ghost to where you have clicked to move
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
            Context.currentlySelectedUnitScript.MoveUnitToGhost();
            Context.isMovementConfirmed = true;
        }
    }
}

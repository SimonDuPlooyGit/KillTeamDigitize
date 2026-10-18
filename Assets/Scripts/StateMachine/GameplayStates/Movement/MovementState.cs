using UnityEngine;
using UnityEngine.InputSystem;

//-----MovementState Class (Handles movement for the player in terms of UI and the confirm move button)-----
public class MovementState : BaseState
{
    //Inherits from BaseState
    //Movement state that handles movement of your units (calls the unit's movement functions)
    
    private readonly InputActions _input; //Needs to use the input system from GameManager
    private readonly MenuPanel _menu;
    public bool MovementFinished;

    public MovementState(InformationPackage context, InputActions input, MenuPanel menu) : base(context) //Needs base(context) for sending context to the BaseState constructor first
    {
        _input = input;
        _menu = menu;
        MovementFinished = false;
    }

    public override void OnEnter()
    {
        Debug.Log("MovementState entered");
        _input.Controls.Move.performed += OnMoveInputPerformed;
        _menu.moveButton2.SetActive(true);
        MovementFinished = false;
        //You can activate move button here

        //Tutorial panel stuff
        if (_menu.tutFlag3)
        {
            _menu.OpenMenu(_menu.tutReposition);
        }
        if (_menu.tutFlag2)
        {
            _menu.tutFlag2 = false; //prevents the action select tut panels from appearing
        }
       
    }

    public override void Update()
    {
        if (Context.CurrentlySelectedUnitScript != null)
        {
            Context.CurrentlySelectedUnitScript.UpdatePathDrawing(); //If there is a unit script update its path drawing
        }

        if (Context.CurrentlySelectedUnitScript != null &&
            Context.CurrentlySelectedUnitScript.remainingMovement < 0.05f)
        {
            Context.IsPlayerActivationRequested = true;
            MovementFinished = true;
        }
    }

    public override void OnExit()
    {
        _input.Controls.Move.performed -= OnMoveInputPerformed;
        Debug.Log("MovementState exited");
        _menu.CloseMenu(_menu.tutReposition);
        _menu.moveButton2?.SetActive(false);
        if (Context.CurrentlySelectedUnitScript.remainingMovement < Context.CurrentlySelectedUnitScript.meterMovement)
        {
            Context.CurrentlySelectedUnitScript.UpdateApl(Context.CurrentlySelectedUnitScript.currentApl -= 1);
        }
        Context.CurrentlySelectedUnitScript.Reset();
    }

    private void OnMoveInputPerformed(InputAction.CallbackContext ctx) //Move the marker to where you have clicked to move
    {
        Debug.Log("OnMoveInputPerformed");
        if (Context.CurrentlySelectedUnitScript != null)
        {
            Debug.Log("Calling ClickToPathfind");
            Context.CurrentlySelectedUnitScript.ClickToPathfind();
        }
        else
        {
            Debug.Log("Context.CurrentlySelectedUnitScript == null");
        }
    }

    public void ConfirmAndExecuteMovement() //Use the confirm move button to confirm moving the unit to marker preview
    {
        if (Context.CurrentlySelectedUnitScript != null)
        {
            Context.CurrentlySelectedUnitScript.MoveOperativeToMarker();
            Context.IsMovementConfirmed = true;
            //set tutorial flags
            if(_menu.tutFlag3)
            {
                _menu.tutFlag3 = false;
            }
            if(_menu.tutFlag4B)
            {
                _menu.tutFlag4B = false;
            }
        }
    }
}

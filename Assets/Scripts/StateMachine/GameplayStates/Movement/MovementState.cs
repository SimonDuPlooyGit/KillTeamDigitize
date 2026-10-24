using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//-----MovementState Class (Handles movement for the player in terms of UI and the confirm move button)-----
public class MovementState : BaseState
{
    //Inherits from BaseState
    //Movement state that handles movement of your units (calls the unit's movement functions)
    
    private readonly InputActions _input; //Needs to use the input system from GameManager
    private readonly MenuPanel _menu;
    private readonly MonoBehaviour _coroutineRunner;
    public bool MovementFinished;
    private bool _isExecutingMove;

    public MovementState(InformationPackage context, InputActions input, MenuPanel menu, MonoBehaviour coroutineRunner) : base(context) //Needs base(context) for sending context to the BaseState constructor first
    {
        _input = input;
        _menu = menu;
        _coroutineRunner = coroutineRunner;
        MovementFinished = false;
    }

    public override void OnEnter()
    {
        Debug.Log("MovementState entered");
        _input.Controls.Move.performed += OnMoveInputPerformed;
        _menu.moveButton2.SetActive(true);
        MovementFinished = false;
        _isExecutingMove = false;
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
        var playerOp = Context.CurrentlySelectedOperativeScript as PlayerOperative;
        if (playerOp != null)
        {
            playerOp.UpdatePathDrawing();
        }
    }

    public override void OnExit()
    {
        _input.Controls.Move.performed -= OnMoveInputPerformed;
        Debug.Log("MovementState exited");
        _menu.CloseMenu(_menu.tutReposition);
        _menu.moveButton2?.SetActive(false);
        
        var playerOp = Context.CurrentlySelectedOperativeScript as PlayerOperative;
        if (playerOp != null)
        {
            playerOp.ResetMoveMarker();
        }
        Context.IsMovementRequested = false;
        Context.SelectedMovement = PlayerMovementType.None;
    }

    private void OnMoveInputPerformed(InputAction.CallbackContext ctx) //Move the marker to where you have clicked to move
    {
        if (_isExecutingMove) return;
        
        var playerOp = Context.CurrentlySelectedOperativeScript as PlayerOperative;
        if (playerOp == null) return;

        switch (Context.SelectedMovement)
        {
            case PlayerMovementType.Reposition:
                playerOp.ClickToPathfindReposition();
                break;
            case PlayerMovementType.Charge:
                playerOp.ClickToPathfindCharge();
                break;
            case PlayerMovementType.Dash:
                playerOp.ClickToPathfindDash();
                break;
        }
    }

    public void ConfirmAndExecuteMovement() //Use the confirm move button to confirm moving the unit to marker preview
    {
        if (_isExecutingMove) return;
        
        var playerOp = Context.CurrentlySelectedOperativeScript as PlayerOperative;
        if (playerOp != null)
        {
            if (playerOp.MoveOperativeToMarker())
            {
                _coroutineRunner.StartCoroutine(ConfirmAndExecuteMovement(playerOp));
            }
            else
            {
                Debug.Log("Movement blocked by invalid destination");
            }
        }
    }

    private IEnumerator ConfirmAndExecuteMovement(PlayerOperative playerOp)
    {
        _isExecutingMove = true;
        yield return new WaitUntil(() => !playerOp.agent.pathPending);

        while (playerOp.agent.remainingDistance > playerOp.agent.stoppingDistance ||
               playerOp.agent.velocity.sqrMagnitude > 0.01f)
        {
            yield return null;
        }
        
        playerOp.UpdateApl(playerOp.currentApl-1);
        playerOp.ResetMoveMarker();
        Context.IsMovementConfirmed = true;
        MovementFinished = true;
    }
}

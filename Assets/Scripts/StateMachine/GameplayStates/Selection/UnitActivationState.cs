using UnityEngine;
using UnityEngine.InputSystem;

//-----UnitActivationState Class (Handles clicking on a player unit to activate them)-----
public class UnitActivationState : BaseState
{
    //Inherits from BaseState
    //Unit activation state that handles the selection and activations of player operatives
    
    private readonly InputActions _input; //Needs access to the input system from GameManager
    private readonly MenuPanel _menu; //Needs access to the menu from GameManager
    private readonly GameManager _gameManager;

    public UnitActivationState(InformationPackage context, InputActions input, MenuPanel menu, GameManager gm) : base(context)
    {
        _input = input;
        _menu = menu;
        _gameManager = gm;
    }
    
    public override void OnEnter()
    {
        _gameManager.CheckTurningPointEnd();
        _gameManager.mainCamera.isFreecam = true;
        //Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("UnitActivationState entered");
        Context.FullReset();
        _input.Controls.Select.performed += OnSelectPerformed;
        _input.Controls.Deselect.performed += OnDeselectPerformed;
        _menu.OpenMenu(_menu.tpCounter);
        _menu.OpenMenu(_menu.objective);

        //tutorial panel
        if(_menu.tutFlag1)
        {
            _menu.OpenMenu(_menu.tutSelect);
            _menu.tutFlag1 = false;
        }
        //temp for reset. remove later
        if(_menu.tutFlag7)
        {
            _menu.OpenMenu(_menu.tutReset);
            _menu.tutFlag7B = false;
        }
        if(!_menu.tutFlag8 && !_menu.tutFlag6B &&_menu.tutFlag7B)
        {
            _menu.OpenMenu(_menu.tutReset);
        }
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
        _menu.CloseMenu(_menu.tutSelect);
        _menu.CloseMenu(_menu.tutReset);

        if(_menu.tutFlag7)
        {
            _menu.tutFlag7 = false;
        }
    }
    
    //If you left-click shoot a raycast and see if hit a friendly unit. If so give it to information package.
    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        if (!Context.PlayerTurn)
        {
            Debug.Log("OnSelectPerformed in UnitActivationState but it's not player's turn");
            return; //Stop selection if it isn't the player's turn Popup not your turn here
        }
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100))
        {
            //Debug.Log("OnSelectPerformed in UnitActivationState Raycast fired");
            if (hit.collider.gameObject.CompareTag("AllyUnit"))
            {
                var unitScript = hit.collider.gameObject.GetComponent<PrototypeUnit>();
                if (unitScript != null && unitScript.acted)
                {
                    //Debug.Log("This unit has already acted in this turning points"); //UI prompt for saying that this unit has acted
                    return;
                }
                
                Context.CurrentlySelectedOperative = hit.collider.gameObject;
                Context.CurrentlySelectedOperativeScript = Context.CurrentlySelectedOperative.GetComponent<PrototypeUnit>();
                Context.ActivatedUnitSo = Context.CurrentlySelectedOperativeScript.operativeData;
                Debug.Log("Selected Operative: " + Context.ActivatedUnitSo.name);
                Context.CurrentlySelectedOperativeScript.AddGlow();
                Context.CurrentlySelectedOperativeScript.selected = true;
                _menu.OpenAction();
                Context.IsPlayerActivationRequested = true;
            }
            else
            {
                //Debug.Log($"Ray did not hit player it hit {hit.collider.gameObject.name}");
            }
        }
    }
    
    private void OnDeselectPerformed(InputAction.CallbackContext ctx) //Deselect the unit that was clicked with right-click
    {
        if (Context.CurrentlySelectedOperativeScript != null)
        {
            Context.CurrentlySelectedOperativeScript.Reset();
            Context.CurrentlySelectedOperativeScript.selected = false;
        }
        Context.FullReset();
    }
}

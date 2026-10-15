using System;
using UnityEngine;
using UnityEngine.SceneManagement; 

//-----GameManager Class-----
public class GameManager : MonoBehaviour
{
    private InputActions _input;
    private InformationPackage _sharedContext; //The script holding information to exchange between all scripts and states
    private StateMachine _stateMachine; //The state machine 
    [SerializeField] private MenuPanel menu;
    [SerializeField] private DiceHandler diceHandler;
    [SerializeField] private OrbitCamera mainCamera;
    [SerializeField] private NPOManager npoManager;

    //Player states
    private UnitActivationState unitActivationState ;
    private ActionSelectionState actionSelectionState;
    private MovementState movementState;
    private TargetingState targetingState;
    private CombatState combatState;
    private WeaponSelectState weaponSelectState;

    //NPO states
    private NPOSelection npoSelectState;
    private NPOActionSelect npoActionSelectState;
    
    //public PauseState pauseState;

    private void Awake()
    {
        _input = new InputActions();
        _stateMachine = new StateMachine();
        _sharedContext = new InformationPackage();
        _sharedContext.MainCameraScript = mainCamera;

        //Initialize states
        unitActivationState = new UnitActivationState(_sharedContext, _input, menu);
        actionSelectionState = new ActionSelectionState(_sharedContext, menu);
        movementState = new MovementState(_sharedContext, _input, menu);
        targetingState = new TargetingState(_sharedContext, _input, menu);
        combatState = new CombatState(_sharedContext, menu, diceHandler);
        weaponSelectState = new WeaponSelectState(_sharedContext, menu);
        npoSelectState = new NPOSelection(_sharedContext, npoManager);
        npoActionSelectState = new NPOActionSelect(_sharedContext);
        //pauseState = new PauseState(_sharedContext, _input, _stateMachine);

        //Define transitions

        //Forward transitions
        AddT(unitActivationState, actionSelectionState, new FuncPredicate(() => _sharedContext.IsPlayerActivationRequested && _sharedContext.CurrentlySelectedUnitScript != null));
        AddT(actionSelectionState, movementState, new FuncPredicate(() => !_sharedContext.IsPlayerActivationRequested && _sharedContext.CurrentlySelectedUnitScript != null));
        AddT(movementState, actionSelectionState, new FuncPredicate(() => movementState.MovementFinished));
        AddT(actionSelectionState, weaponSelectState, new FuncPredicate(() => _sharedContext.IsShootingRequested));
        AddT(weaponSelectState, targetingState, new FuncPredicate(() => _sharedContext.IsWeaponSelected));
        AddT(targetingState, combatState, new FuncPredicate(() => _sharedContext.ValidTarget));
        AddT(combatState, unitActivationState, new FuncPredicate(() => _sharedContext.IsShootingConfirmed));
        AddT(actionSelectionState, npoSelectState, new FuncPredicate(() => _sharedContext.CurrentlySelectedUnitScript.acted));
        AddT(npoSelectState, npoActionSelectState, new FuncPredicate(() => _sharedContext.CurrentlyActivatedNpo != null));
        
        //AddT for pressing a pass turn button
        _stateMachine.SetState(unitActivationState);

        //Backwards transitions
        GoBackAState(actionSelectionState, unitActivationState, () =>
        {
            _sharedContext.CurrentlySelectedUnitScript.selected = false;
            _sharedContext.CurrentlySelectedUnitScript.Reset();
            _sharedContext.Reset();
        });
        GoBackAState(movementState, actionSelectionState, () =>
        {
            _sharedContext.IsPlayerActivationRequested = true;
        });
        GoBackAState(weaponSelectState, actionSelectionState, () =>
        {
            _sharedContext.IsShootingRequested = false;
        });
        GoBackAState(targetingState, weaponSelectState, () =>
        {
            _sharedContext.IsWeaponSelected = false;
            _sharedContext.Weapon = null;
        });
        GoBackAState(combatState, targetingState, () =>
        {
            _sharedContext.CurrentlySelectedTarget = null;
        });
        
        //Global any transition
        //AnyT(pauseState, new FuncPredicate(() => _input.Controls.Pause.WasPressedThisFrame() && _stateMachine.CurrentState is not PauseState && Time.frameCount > _stateMachine.LastTransitionFrame));
    }

    //Helper function to do backwards transitions with right click as the predicate
    ////The action onBackTransition is logic you want to happen on the transition such as closing menus
    public void GoBackAState(IState currentState, IState previousState, Action onBackTransition) 
    {
        AddT(currentState, previousState, new FuncPredicate(() =>
        {
            if (_input.Controls.Deselect.WasPressedThisFrame())
            {
                onBackTransition?.Invoke();
                return true;
            }
            return false;
        }));
    }
    
    //Helper function to add a transition
    void AddT(IState from, IState to, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
    //Helper function to add a transition from any state
    void AnyT(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }
        
        _stateMachine.Update(); //Keep the state machine updating
        if (_stateMachine.CurrentState == weaponSelectState)
        {
            mainCamera.switchOffForMenu = true;
        }
        else
        {
            mainCamera.switchOffForMenu = false;
        }
    }

    public void OnMoveActionButtonPressed()
    {
        _sharedContext.IsPlayerActivationRequested = false;
    }
    
    public void OnConfirmMovementButtonPressed()
    {
        if (movementState != null)
        {
            movementState.ConfirmAndExecuteMovement();
        }
    }

    public void OnShootingButtonPressed()
    {
        _sharedContext.IsShootingRequested = true;
    }
    
    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    public void Quit()
    {
        Application.Quit();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnEndTurnPressed()
    {
        _sharedContext.CurrentlySelectedUnitScript.acted = true;
    }
}
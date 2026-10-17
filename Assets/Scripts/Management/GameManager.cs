using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

//-----GameManager Class-----
public class GameManager : MonoBehaviour
{
    private InputActions _input;
    public InformationPackage SharedContext; //The script holding information to exchange between all scripts and states
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
        SharedContext = new InformationPackage();
        SharedContext.MainCameraScript = mainCamera;
        SharedContext.AllPlayerUnits =
            new List<PrototypeUnit>(FindObjectsByType<PrototypeUnit>(FindObjectsSortMode.None));
        SharedContext.AllNpoUnits = new List<PrototypeNPO>(FindObjectsByType<PrototypeNPO>(FindObjectsSortMode.None));

        //Initialize states
        unitActivationState = new UnitActivationState(SharedContext, _input, menu);
        actionSelectionState = new ActionSelectionState(SharedContext, menu);
        movementState = new MovementState(SharedContext, _input, menu);
        targetingState = new TargetingState(SharedContext, _input, menu);
        combatState = new CombatState(SharedContext, menu, diceHandler);
        weaponSelectState = new WeaponSelectState(SharedContext, menu);
        npoSelectState = new NPOSelection(SharedContext, npoManager);
        npoActionSelectState = new NPOActionSelect(SharedContext);
        //pauseState = new PauseState(SharedContext, _input, _stateMachine);

        //Define transitions

        //Forward transitions
        AddT(unitActivationState, actionSelectionState,
            new FuncPredicate(() =>
                SharedContext.IsPlayerActivationRequested && SharedContext.CurrentlySelectedUnitScript != null));
        AddT(actionSelectionState, movementState,
            new FuncPredicate(() =>
                !SharedContext.IsPlayerActivationRequested && SharedContext.CurrentlySelectedUnitScript != null));
        AddT(movementState, actionSelectionState, new FuncPredicate(() => movementState.MovementFinished));
        AddT(actionSelectionState, weaponSelectState, new FuncPredicate(() => SharedContext.IsShootingRequested));
        AddT(weaponSelectState, targetingState, new FuncPredicate(() => SharedContext.IsWeaponSelected));
        AddT(targetingState, combatState, new FuncPredicate(() => SharedContext.ValidTarget));
        AddT(combatState, unitActivationState, new FuncPredicate(() => SharedContext.IsShootingConfirmed));
        AddT(actionSelectionState, npoSelectState, new FuncPredicate(() => !SharedContext.PlayerTurn));
        AddT(npoSelectState, npoActionSelectState,
            new FuncPredicate(() => SharedContext.CurrentlyActivatedNpo != null)); //If NPO is found to activate
        AddT(npoActionSelectState, unitActivationState, new FuncPredicate(() => SharedContext.PlayerTurn));


        //AddT for pressing a pass turn button
        _stateMachine.SetState(unitActivationState);

        //Backwards transitions
        GoBackAState(actionSelectionState, unitActivationState,
            () =>
            {
                if (SharedContext.CurrentlySelectedUnitScript.currentApl ==
                    SharedContext.CurrentlySelectedUnitScript.operativeData.apl)
                {
                    return true;
                }
                else
                {
                    Debug.Log("Can't swap from this unit because it made an action already");
                    return false;
                }
            },
            () =>
            {
            SharedContext.CurrentlySelectedUnitScript.selected = false;
            SharedContext.CurrentlySelectedUnitScript.Reset();
            SharedContext.Reset();
            }
        );

        GoBackAState(movementState, actionSelectionState,()=>true,() =>
        {
            SharedContext.IsPlayerActivationRequested = true;
        });
        GoBackAState(weaponSelectState, actionSelectionState, ()=>true,() =>
        {
            SharedContext.IsShootingRequested = false;
        });
        GoBackAState(targetingState, weaponSelectState,()=>true,() =>
        {
            SharedContext.IsWeaponSelected = false;
            SharedContext.Weapon = null;
        });
        GoBackAState(combatState, targetingState,()=>true,() =>
        {
            SharedContext.CurrentlySelectedTarget = null;
        });
        
        //Global any transition
        //AnyT(pauseState, new FuncPredicate(() => _input.Controls.Pause.WasPressedThisFrame() && _stateMachine.CurrentState is not PauseState && Time.frameCount > _stateMachine.LastTransitionFrame));
    }

    //Helper function to do backwards transitions with right click as the predicate
    ////The action onBackTransition is logic you want to happen on the transition such as closing menus
    public void GoBackAState(IState currentState, IState previousState, Func<bool> canGoBack, Action onBackTransition) 
    {
        AddT(currentState, previousState, new FuncPredicate(() =>
        {
            if (_input.Controls.Deselect.WasPressedThisFrame())
            {
                if (!canGoBack())
                {
                    return false;
                }
                
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
        SharedContext.IsPlayerActivationRequested = false;
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
        SharedContext.IsShootingRequested = true;
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
        if (SharedContext.CurrentlySelectedUnitScript != null)
        {
            SharedContext.CurrentlySelectedUnitScript.acted = true;
            SharedContext.CurrentlySelectedUnitScript.selected = false;
            SharedContext.CurrentlySelectedUnitScript.Reset();
        }
        
        SharedContext.Reset();
        SharedContext.PlayerTurn = false;
        CheckTurningPointEnd();
    }

    public void CheckTurningPointEnd()
    {
        bool playerUnitsLeft = SharedContext.AllPlayerUnits.Exists(u => !u.dead && !u.acted);
        bool nposLeft = SharedContext.AllNpoUnits.Exists(n => !n.dead && !n.acted);

        if (!playerUnitsLeft && !nposLeft)
        {
            SharedContext.CurrentTurningPoint++;
            if (SharedContext.CurrentTurningPoint > InformationPackage.LastTurningPoint)
            {
                Debug.Log("Last turning point ended");
                return;
            }
            
            Debug.Log($"Starting turning point {SharedContext.CurrentTurningPoint}");

            foreach (var unit in SharedContext.AllPlayerUnits)
            {
                unit.acted = false;
                unit.currentApl = unit.operativeData.apl;
                unit.selected = false;
            }

            foreach (var npo in SharedContext.AllNpoUnits)
            {
                npo.acted = false;
                npo.currentApl = npo.operativeData.apl;
                npo.selected = false;
            }
        }

        if (SharedContext.PlayerTurn && !playerUnitsLeft)
        {
            SharedContext.PlayerTurn = false;
        } else if (!SharedContext.PlayerTurn && !nposLeft)
        {
            SharedContext.PlayerTurn = true;
        }
    }
}
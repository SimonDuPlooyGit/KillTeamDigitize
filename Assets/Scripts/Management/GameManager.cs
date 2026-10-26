using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

//-----GameManager Class-----
public class GameManager : MonoBehaviour
{
    private InputActions _input;
    public InformationPackage SharedContext; //The script holding information to exchange between all scripts and states
    private StateMachine _stateMachine; //The state machine 
    [SerializeField] private MenuPanel menu;
    [SerializeField] public DiceHandler diceHandler;
    [SerializeField] public OrbitCamera mainCamera;
    [SerializeField] private NPOManager npoManager;
    [SerializeField] public GameSetup setup;

    //Player states
    private UnitActivationState unitActivationState;
    private ActionSelectionState actionSelectionState;
    private MovementState movementState;
    private TargetingState targetingState;
    private ShootCombatState shootCombatState;
    private WeaponSelectState weaponSelectState;
    private IntroductionState introductionState;
    private MeleeCombatState meleeCombatState;

    //NPO states
    private NPOSelection npoSelectState;
    private NPOActionSelect npoTurnPerformState;
    
    //public PauseState pauseState;

    private void Awake()
    {
        SharedContext = new InformationPackage();
        _input = new InputActions();
        _stateMachine = new StateMachine();
        SharedContext.MainCameraScript = mainCamera;
        mainCamera.isFreecam = true;
        SharedContext.AllPlayerUnits = new List<PlayerOperative>(FindObjectsByType<PlayerOperative>(FindObjectsSortMode.None));
        SharedContext.AllNpoUnits = new List<NonPlayerOperative>(FindObjectsByType<NonPlayerOperative>(FindObjectsSortMode.None));

        //Initialize states
        introductionState = new IntroductionState(SharedContext, this);
        unitActivationState = new UnitActivationState(SharedContext, _input, menu, this);
        actionSelectionState = new ActionSelectionState(SharedContext, menu, this);
        movementState = new MovementState(SharedContext, _input, menu, this);
        targetingState = new TargetingState(SharedContext, _input, menu);
        shootCombatState = new ShootCombatState(SharedContext, menu, diceHandler);
        weaponSelectState = new WeaponSelectState(SharedContext, menu);
        npoSelectState = new NPOSelection(SharedContext, npoManager, this);
        npoTurnPerformState = new NPOActionSelect(SharedContext);
        meleeCombatState = new MeleeCombatState(SharedContext, menu, diceHandler);
        //pauseState = new PauseState(SharedContext, _input, _stateMachine);

        //Define transitions

        //Forward transitions
        AddT(introductionState, unitActivationState, new FuncPredicate(() => introductionState.MoveToFirstTurn));
        AddT(unitActivationState, actionSelectionState,
            new FuncPredicate(() =>
                SharedContext.IsPlayerActivationRequested && SharedContext.CurrentlySelectedOperativeScript != null));
        AddT(actionSelectionState, movementState,
            new FuncPredicate(() =>
                SharedContext.IsMovementRequested && SharedContext.CurrentlySelectedOperativeScript != null));
        AddT(movementState, actionSelectionState, new FuncPredicate(() => movementState.MovementFinished));
        AddT(actionSelectionState, weaponSelectState, new FuncPredicate(() => SharedContext.IsShootingRequested || SharedContext.IsFightingRequested));
        AddT(weaponSelectState, targetingState, new FuncPredicate(() => SharedContext.IsWeaponSelected));
        AddT(targetingState, shootCombatState, new FuncPredicate(() => SharedContext.ValidTargetShoot));
        AddT(targetingState, meleeCombatState, new FuncPredicate(() => SharedContext.ValidTargetFight));
        AddT(shootCombatState, actionSelectionState, new FuncPredicate(() =>
        {
            if (SharedContext.IsShootingConfirmed)
            {
                SharedContext.IsShootingRequested = false;
                SharedContext.IsWeaponSelected = false;
                SharedContext.ValidTargetShoot = false;
                SharedContext.IsShootingConfirmed = false;
                return true;
            }

            return false;
        }));
        AddT(actionSelectionState, npoSelectState, new FuncPredicate(() => !SharedContext.PlayerTurn));
        AddT(npoSelectState, npoTurnPerformState,
            new FuncPredicate(() => SharedContext.CurrentlySelectedOperativeScript != null && SharedContext.CurrentlySelectedOperativeScript is NonPlayerOperative)); //If NPO is found to activate
        AddT(npoTurnPerformState, unitActivationState, new FuncPredicate(() => SharedContext.PlayerTurn));
        AddT(npoTurnPerformState, meleeCombatState, new FuncPredicate(() => SharedContext.IsFightingRequested && SharedContext.CurrentlySelectedOperativeScript is NonPlayerOperative));AddT(meleeCombatState, npoTurnPerformState, new FuncPredicate(() => 
        {
            if (SharedContext.IsFightingRequested && SharedContext.CurrentlySelectedOperativeScript is NonPlayerOperative)
            {
                SharedContext.IsFightingRequested = false;
                return true;
            }
            return false;
        }));
        


        //AddT for pressing a pass turn button
        _stateMachine.SetState(unitActivationState);

        //Backwards transitions
        GoBackAState(actionSelectionState, unitActivationState,
            () =>
            {
                if (SharedContext.CurrentlySelectedOperativeScript.currentApl ==
                    SharedContext.CurrentlySelectedOperativeScript.operativeData.apl)
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
            SharedContext.CurrentlySelectedOperativeScript.Deselect();
            SharedContext.FullReset();
            }
        );

        GoBackAState(movementState, actionSelectionState,()=>true,() =>
        {
            SharedContext.IsMovementRequested = false;
            SharedContext.SelectedMovement = PlayerMovementType.None;
            var playerOp = SharedContext.CurrentlySelectedOperativeScript as PlayerOperative;
            if (playerOp != null)
            {
                playerOp.ResetMoveMarker();
            }
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
        GoBackAState(shootCombatState, targetingState,()=>true,() =>
        {
            SharedContext.CurrentlySelectedTarget = null;
        });

        menu.tpCounter.GetComponentInChildren<TextMeshProUGUI>().text = "1";

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

    public void OnRepositionActionButtonPressed()
    {
        SharedContext.SelectedMovement = PlayerMovementType.Reposition;
        SharedContext.IsMovementRequested = true;
    }

    public void OnChargeActionButtonPressed()
    {
        SharedContext.SelectedMovement = PlayerMovementType.Charge;
        SharedContext.IsMovementRequested = true;
    }

    public void OnDashActionButtonPressed()
    {
        SharedContext.SelectedMovement = PlayerMovementType.Dash;
        SharedContext.IsMovementRequested = true;
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

    public void OnFightButtonPressed()
    {
        SharedContext.IsFightingRequested = true;
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
        if (SharedContext.CurrentlySelectedOperativeScript != null)
        {
            SharedContext.CurrentlySelectedOperativeScript.acted = true;
            SharedContext.CurrentlySelectedOperativeScript.selected = false;
            SharedContext.CurrentlySelectedOperativeScript.TurnFinished();
        }
        
        SharedContext.FullReset();
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
            menu.tpCounter.GetComponentInChildren<TextMeshProUGUI>().text = SharedContext.CurrentTurningPoint.ToString();
            if (SharedContext.CurrentTurningPoint > InformationPackage.LastTurningPoint)
            {
                Debug.Log("Last turning point ended");
                return;
            }
            
            Debug.Log($"Starting turning point {SharedContext.CurrentTurningPoint}");

            foreach (var unit in SharedContext.AllPlayerUnits)
            {
                unit.ResetForNewTurningPoint();
            }

            foreach (var npo in SharedContext.AllNpoUnits)
            {
                npo.ResetForNewTurningPoint();
            }
            
            SharedContext.PlayerTurn = true;
            return;
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